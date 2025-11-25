import json
import sys
import argparse
from ia.sentiment_analyzer import SentimentAnalyzer
from ia.model_loader import initialize_model
from ia.response_analyzer import SurveyAnalyzer


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("input", nargs="?", help="Arquivo JSON de entrada (ou STDIN)")
    parser.add_argument("--topk", type=int, default=5, help="Top-K palavras-chave (padrão 5)")
    args = parser.parse_args()

    # Lê JSON do arquivo ou STDIN
    if args.input:
        with open(args.input, "r", encoding="utf-8") as fh:
            payload = json.load(fh)
    else:
        payload = json.load(sys.stdin)

    model = initialize_model()
    analyzer = SentimentAnalyzer(model)
    survey = SurveyAnalyzer(analyzer)
    result = survey.analyze(payload, top_k=args.topk)
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
