from collections import Counter
from typing import Dict, Any, List, Tuple
import re

from ia.sentiment_analyzer import SentimentAnalyzer


DEFAULT_STOPWORDS = set(
    [
        # PT-BR
        "a","o","os","as","um","uma","de","do","da","dos","das","em","no","na","nos","nas","para","por","com","sem","e","ou","mas","que","se","ao","à","às","aos","àquilo","isso","isso","aquilo","é","foi","era","ser","estar","são","sua","seu","suas","seus","minha","meu","minhas","meus",
        # EN
        "the","a","an","and","or","but","to","of","in","on","at","for","with","without","is","are","was","were","be","been","being","this","that","these","those","it","its","as","by","from","about","into","over","after","before","between","out","up","down","so","than",
    ]
)


def normalize_text(t: str) -> List[str]:
    """Lowercase, remove diacritics basics, split tokens, filter short tokens."""
    if not t:
        return []
    s = t.lower()
    # remove pontuação (compatível com Python re)
    s = re.sub(r"[^\w\s]", " ", s, flags=re.UNICODE)
    # remover múltiplos espaços
    s = re.sub(r"\s+", " ", s).strip()
    tokens = [w for w in s.split(" ") if len(w) >= 3 and w not in DEFAULT_STOPWORDS]
    return tokens


def extract_keywords(texts: List[str], top_k: int = 10) -> List[Tuple[str, int]]:
    cnt = Counter()
    for t in texts:
        cnt.update(normalize_text(t))
    return cnt.most_common(top_k)


class SurveyAnalyzer:
    """
    Analisa um JSON de respostas de pesquisa e retorna métricas agregadas.

    Contrato:
    - Entrada (dict):
      {
        "responses": [
           {"id": "1", "text": "..."},
           {"id": "2", "text": "..."}
        ]
      }
    - Saída (dict):
      {
        "summary": {"positive": n1, "negative": n2, "neutral": n3},
        "keywords": [["palavra", count], ...],
        "items": [
          {"id": "1", "sentiment": "positive", "category": "elogio"}, ...
        ]
      }
    """

    def __init__(self, analyzer: SentimentAnalyzer):
        self.analyzer = analyzer

    def analyze(self, payload: Dict[str, Any], top_k: int = 10) -> Dict[str, Any]:
        items = payload.get("responses") or []
        results = []
        sentiments = Counter()
        categories = Counter()
        texts = []
        word_counts = []
        
        for it in items:
            text = (it.get("text") or "").strip()
            sid = it.get("id")
            if not text:
                continue
            
            sent = self.analyzer.analyze_sentiment(text)
            cat = self.analyzer.classify_category(text)
            word_count = len(text.split())
            
            results.append({
                "id": sid, 
                "sentiment": sent, 
                "category": cat,
                "text": text,
                "word_count": word_count
            })
            
            sentiments.update([sent])
            categories.update([cat])
            texts.append(text)
            word_counts.append(word_count)

        keywords = extract_keywords(texts, top_k=top_k)
        
        # Cálculos analíticos
        total_responses = len(results)
        avg_word_count = sum(word_counts) / len(word_counts) if word_counts else 0
        
        # Percentuais de sentimento
        sentiment_percentages = {}
        for sent, count in sentiments.items():
            sentiment_percentages[sent] = round((count / total_responses) * 100, 2)
        
        # Análise por categoria
        category_analysis = {}
        for cat, count in categories.items():
            cat_sentiments = Counter()
            for item in results:
                if item["category"] == cat:
                    cat_sentiments.update([item["sentiment"]])
            
            category_analysis[cat] = {
                "total": count,
                "percentage": round((count / total_responses) * 100, 2),
                "sentiments": dict(cat_sentiments)
            }
        
        return {
            "analytics": {
                "positive": sentiments.get("positive", 0),
                "negative": sentiments.get("negative", 0),
                "neutral": sentiments.get("neutral", 0),
                "total": total_responses
            },
            "keywords": keywords
        }
