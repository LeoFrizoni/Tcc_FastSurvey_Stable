class SentimentAnalyzer:
    def __init__(self, model=None):
        self.model = model

    def analyze_sentiment(self, text):
        # Se houver um modelo, delega a ele. Caso contrário, usa heurística simples.
        if self.model is not None:
            try:
                pred = self.model.predict(text)
                # Normaliza possíveis labels da HF (e.g., POSITIVE, NEGATIVE, NEUTRAL)
                if isinstance(pred, str):
                    p = pred.strip().lower()
                    if p in {"positive", "neg", "negativo", "negative", "neutral", "neutro", "pos", "positivo"}:
                        if p.startswith("pos"):
                            return "positive"
                        if p.startswith("neg") or p == "negativo" or p == "negative":
                            return "negative"
                        return "neutral"
                # Caso o modelo retorne um índice
                if isinstance(pred, int):
                    return {0: "negative", 1: "neutral", 2: "positive"}.get(pred, "neutral")
                # Caso retorne dict com label
                if isinstance(pred, dict) and "label" in pred:
                    return str(pred["label"]).strip().lower()
            except Exception:
                pass

        txt = (text or "").lower()
        positives = [
            "amo", "adoro", "ótimo", "otimo", "fantástico", "fantastico", "excelente", "maravilhoso",
            "bom", "perfeito", "satisfeito", "feliz", "worth", "amazing", "great", "love",
        ]
        negatives = [
            "odeio", "pior", "terrível", "terrivel", "decepcionado", "ruim", "horrível", "horrivel",
            "desperdício", "desperdicio", "nunca", "insuportável", "waste", "bad", "hate", "terrible",
        ]

        score = 0
        for w in positives:
            if w in txt:
                score += 1
        for w in negatives:
            if w in txt:
                score -= 1

        if score > 0:
            return "positive"
        if score < 0:
            return "negative"
        return "neutral"

    def classify_category(self, text: str) -> str:
        """Classifica a resposta por categoria de intenção.

        Categorias previstas:
        - elogio
        - critica (ou reclamação)
        - sugestao
        - duvida
        - neutra
        """
        import re
        t = (text or "").lower()
        # palavras-chave por categoria (PT-BR e alguns termos EN)
        elogio = [
            "amo", "adoro", "gostei", "ótimo", "otimo", "excelente", "maravilhoso", "perfeito",
            "recomendo", "fantástico", "fantastico", "amazing", "great", "love", "feliz", "satisfeito",
        ]
        critica = [
            "odeio", "pior", "terrível", "terrivel", "horrível", "horrivel", "ruim", "decepcionado",
            "péssimo", "pessimo", "nunca mais", "insatisfeito", "terrible", "hate", "awful", "bad",
        ]
        sugestao = ["poderia", "seria bom", "sugestão", "sugestao", "melhorar", "deveria", "recomendo que", "poderiam"]
        palpite = ["acho", "creio", "penso", "talvez", "na minha opinião", "na minha opiniao", "imagino"]
        # usar padrões com bordas de palavra para dúvidas
        duvida_patterns = [r"\bcomo\b", r"\bquando\b", r"\bpor\s*que\b", r"\bporque\b", r"\bonde\b", r"\bqual\b", r"\bfunciona\b", r"\bsuporte\b", r"\bhelp\b"]

        def any_in(words):
            return any(w in t for w in words)

        if any_in(elogio):
            return "elogio"
        if any_in(critica):
            return "critica"
        if any_in(sugestao):
            return "sugestao"
        if any_in(palpite):
            return "palpite"
        if any(re.search(p, t) for p in duvida_patterns) or t.strip().endswith("?"):
            return "duvida"
        return "neutra"

    def categorize_responses(self, responses):
        results = []
        for response in responses:
            sentiment = self.analyze_sentiment(response)
            results.append({
                'response': response,
                'sentiment': sentiment
            })
        return results