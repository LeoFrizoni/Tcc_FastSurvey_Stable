import unittest
from ia.sentiment_analyzer import SentimentAnalyzer

class TestSentimentAnalyzer(unittest.TestCase):

    def setUp(self):
        self.analyzer = SentimentAnalyzer()

    def test_analyze_positive_sentiment(self):
        response = "Eu amo este produto! É incrível."
        result = self.analyzer.analyze_sentiment(response)
        self.assertEqual(result, 'positive')

    def test_analyze_negative_sentiment(self):
        response = "Eu odeio este produto. É terrível."
        result = self.analyzer.analyze_sentiment(response)
        self.assertEqual(result, 'negative')

    def test_analyze_neutral_sentiment(self):
        response = "Este produto é razoável."
        result = self.analyzer.analyze_sentiment(response)
        self.assertEqual(result, 'neutral')

if __name__ == '__main__':
    unittest.main()