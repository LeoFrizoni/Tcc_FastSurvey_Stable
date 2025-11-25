class Response:
    def __init__(self, id, text, sentiment):
        self.id = id
        self.text = text
        self.sentiment = sentiment

    def __repr__(self):
        return f"<Response(id={self.id}, text='{self.text}', sentiment='{self.sentiment}')>"