# main.py

import os
from typing import Optional
from ia.sentiment_analyzer import SentimentAnalyzer
from ia.model_loader import initialize_model
from ia.database.sqlalchemy_db import get_session_factory, ResponseResult
from config.settings import MODEL_PATH, SAMPLE_RESPONSES_FILE


def get_analyzer() -> SentimentAnalyzer:
    # Tenta carregar um modelo; se não houver caminho, usa analisador dummy
    model = None
    if MODEL_PATH:
        try:
            model = initialize_model()
        except Exception as e:
            print(f"Aviso: falha ao carregar modelo em '{MODEL_PATH}': {e}. Usando heurística simples.")
    return SentimentAnalyzer(model)  # analyzer deve lidar com model None


def load_responses(path: Optional[str] = None):
    base_dir = os.path.dirname(os.path.abspath(__file__))
    file_path = path or SAMPLE_RESPONSES_FILE
    if not os.path.isabs(file_path):
        file_path = os.path.join(base_dir, file_path)
    if not os.path.exists(file_path):
        print(f"Arquivo de respostas não encontrado: {file_path}")
        return []
    with open(file_path, "r", encoding="utf-8") as fh:
        return [ln.strip() for ln in fh.readlines() if ln.strip()]


def store_response(session, text: str, sentiment: str, category: str):
    item = ResponseResult(text=text, sentiment=sentiment, category=category)
    session.add(item)


def main():
    SessionFactory = get_session_factory()
    analyzer = get_analyzer()

    responses = load_responses()
    if not responses:
        print("Nenhuma resposta para processar.")
        return

    with SessionFactory() as session:
        for response in responses:
            sentiment = analyzer.analyze_sentiment(response)
            category = analyzer.classify_category(response)
            print(f"Mensagem: {response}\n  Sentimento: {sentiment} | Categoria: {category}")
            store_response(session, response, sentiment, category)
        session.commit()
        print(f"Processadas {len(responses)} respostas.")


if __name__ == "__main__":
    main()