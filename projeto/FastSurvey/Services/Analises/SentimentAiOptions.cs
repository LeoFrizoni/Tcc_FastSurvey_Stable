namespace FASTSURVEY.Services.Analises
{
    public sealed class SentimentAiOptions
    {
        public const string SectionName = "SentimentAI";
        public bool Enabled { get; set; } = true;
        public string PythonPath { get; set; } = "python";
        public string WorkingDirectory { get; set; } = "../sentiment-analysis-ai";
        public string Script { get; set; } = "scripts/analyze_response.py";
        public int TimeoutSeconds { get; set; } = 25;
    }
}
