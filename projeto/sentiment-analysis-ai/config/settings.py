# settings.py

import os

# URL do banco de dados.
# Padrão: usa a conexão do appsettings.json do FastSurvey (PostgreSQL local)
# Formato SQLAlchemy/psycopg2: postgresql+psycopg2://usuario:senha@host:porta/banco
DATABASE_URL = os.getenv(
	"SENTIMENT_DB_URL",
	"postgresql+psycopg2://postgres:admin@localhost:5432/FastSurvey",
)

# Caminho para um modelo pré-treinado (opcional, legado PyTorch .pt/.pth). Se vazio, usa analisador heurístico.
MODEL_PATH = os.getenv("SENTIMENT_MODEL_PATH", "")

# Integração com modelos Hugging Face (LLaMA/TinyLlama) – para treino/inferência
# ID do modelo base (ex.: "meta-llama/Llama-3.2-1B" ou uma alternativa aberta como
# "TinyLlama/TinyLlama-1.1B-Chat-v1.0"). Se vazio, não usa HF.
HF_BASE_MODEL_ID = os.getenv("SENTIMENT_HF_BASE_MODEL_ID", "")
# Caminho do adaptador LoRA gerado no treino (ex.: "models/llama-sentiment-adapter").
HF_ADAPTER_PATH = os.getenv("SENTIMENT_HF_ADAPTER_PATH", "")
# Dispositivo de execução para HF ("auto", "cpu", "cuda").
HF_DEVICE = os.getenv("SENTIMENT_HF_DEVICE", "auto")

# Nível de log
LOG_LEVEL = os.getenv("SENTIMENT_LOG_LEVEL", "INFO")

# Arquivo de respostas exemplo
SAMPLE_RESPONSES_FILE = os.getenv("SENTIMENT_INPUT_FILE", "data/sample_responses.txt")

# Nome da tabela de resultados
RESULTS_TABLE_NAME = os.getenv("SENTIMENT_RESULTS_TABLE", "Analises")