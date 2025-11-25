"""CLI para analisar uma resposta discursiva individual.

Uso:
    python scripts/analyze_response.py --resposta-id 123 --texto "Gostei muito do atendimento"

O script calcula sentimento/categoria, gera palavras-chave e persiste o resultado
na tabela Analises do FastSurvey. O JSON final é impresso no stdout para consumo
por outros serviços.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Dict, List, Tuple

CURRENT_DIR = Path(__file__).resolve().parent
PROJECT_ROOT = CURRENT_DIR.parent
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from ia.model_loader import initialize_model
from ia.response_analyzer import extract_keywords
from ia.sentiment_analyzer import SentimentAnalyzer
from ia.database.analysis_store import AnalysisStore


def _detect_language(text: str) -> str:
    text_lower = text.lower()
    score_pt = sum(1 for token in ("não", "voce", "você", "pra", "mais", "muito", "ótimo", "otimo") if token in text_lower)
    score_en = sum(1 for token in ("the", "and", "this", "that", "very", "good", "bad") if token in text_lower)
    return "pt" if score_pt >= score_en else "en"


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Analisa o sentimento de uma resposta discursiva")
    parser.add_argument("--resposta-id", type=int, required=True, help="ID da resposta criada no FastSurvey")
    parser.add_argument("--texto", type=str, default="", help="Conteúdo textual completo enviado pelo respondente")
    parser.add_argument("--input-file", type=str, default="", help="Arquivo alternativo contendo o texto (UTF-8)")
    parser.add_argument("--meta", type=str, default="{}", help="JSON opcional com metadados adicionais (ex.: perguntaId)")
    parser.add_argument("--keywords", type=int, default=8, help="Quantidade máxima de palavras-chave a extrair")
    return parser


def main(argv: List[str] | None = None) -> int:
    parser = _build_parser()
    args = parser.parse_args(argv)

    if args.input_file:
        texto = Path(args.input_file).read_text(encoding="utf-8").strip()
    else:
        texto = (args.texto or "").strip()
    if not texto:
        print(json.dumps({"status": "skipped", "reason": "texto vazio"}, ensure_ascii=False))
        return 0

    try:
        metadata: Dict[str, Any] = json.loads(args.meta)
    except json.JSONDecodeError:
        metadata = {}

    # Inicializa o modelo apenas se disponível
    model = None
    try:
        model = initialize_model()
    except Exception as exc:  # pragma: no cover - dependente de ambiente HF
        print(json.dumps({"status": "warn", "message": f"Falha ao carregar modelo dedicado: {exc}"}, ensure_ascii=False), file=sys.stderr)

    analyzer = SentimentAnalyzer(model)
    sentiment = analyzer.analyze_sentiment(texto)
    category = analyzer.classify_category(texto)
    keywords = extract_keywords([texto], top_k=max(1, args.keywords))

    analytics_payload = {
        "sentiment": sentiment,
        "category": category,
        "wordCount": len(texto.split()),
        "charCount": len(texto),
        "language": _detect_language(texto),
        "metadata": metadata,
    }

    store = AnalysisStore()
    store.upsert(args.resposta_id, analytics_payload, keywords)

    print(
        json.dumps(
            {
                "status": "ok",
                "respostaId": args.resposta_id,
                "analytics": analytics_payload,
                "keywords": keywords,
            },
            ensure_ascii=False,
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
