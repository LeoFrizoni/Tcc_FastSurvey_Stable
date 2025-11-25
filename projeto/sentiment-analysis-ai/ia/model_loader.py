from typing import Optional, Any


def load_model(model_path):
    """
    Carrega um modelo PyTorch salvo (formato .pt/.pth). Caminho legado.
    """
    import torch
    model = torch.load(model_path, map_location="cpu")
    if hasattr(model, "eval"):
        model.eval()
    return model

def get_model_path():
    """Recupera o caminho do modelo legado (arquivo torch)."""
    from config.settings import MODEL_PATH
    return MODEL_PATH

def initialize_model():
    """
    Inicializa o modelo para análise de sentimento.

    Ordem de tentativa:
    1) Hugging Face (HF_BASE_MODEL_ID + HF_ADAPTER_PATH), retornando um wrapper com método predict(text)
    2) Modelo legado torch (MODEL_PATH)
    """
    from config.settings import HF_BASE_MODEL_ID, HF_ADAPTER_PATH, HF_DEVICE
    # Tenta HF primeiro
    if HF_BASE_MODEL_ID:
        try:
            model = initialize_hf_pipeline(HF_BASE_MODEL_ID, HF_ADAPTER_PATH or None, device=HF_DEVICE)
            return model
        except Exception as e:
            print(f"Aviso: falha ao inicializar HF model '{HF_BASE_MODEL_ID}': {e}")
    # Fallback: arquivo torch
    model_path = get_model_path()
    if model_path:
        try:
            return load_model(model_path)
        except Exception as e:
            print(f"Aviso: falha ao carregar modelo PyTorch '{model_path}': {e}")
    return None


def initialize_hf_pipeline(base_model_id: str, adapter_path: Optional[str] = None, device: str = "auto"):
    """
    Inicializa uma pipeline Hugging Face para classificação de sentimento usando LLaMA/TinyLlama com PEFT (LoRA) opcional.

    Retorna um objeto com método predict(text) -> str (label em 'positive'/'negative'/'neutral').
    """
    from transformers import AutoModelForSequenceClassification, AutoTokenizer
    from peft import PeftModel
    import torch

    # Resolve device
    if device == "auto":
        device_idx = 0 if torch.cuda.is_available() else -1
    elif device == "cpu":
        device_idx = -1
    else:
        # "cuda" assume idx 0
        device_idx = 0

    tokenizer = AutoTokenizer.from_pretrained(base_model_id, use_fast=True)
    model = AutoModelForSequenceClassification.from_pretrained(base_model_id, num_labels=3)
    if adapter_path:
        model = PeftModel.from_pretrained(model, adapter_path)

    id2label = getattr(model.config, "id2label", {0: "negative", 1: "neutral", 2: "positive"})
    label2id = {str(v).lower(): int(k) for k, v in id2label.items()}

    class HFWrapper:
        def __init__(self, mdl: Any, tok: Any, dev_idx: int):
            self.model = mdl
            self.tokenizer = tok
            self.device_idx = dev_idx
            if dev_idx >= 0:
                self.model = self.model.to(f"cuda:{dev_idx}")
            self.model.eval()

        def predict(self, text: str) -> str:
            if not text:
                return "neutral"
            import torch
            with torch.no_grad():
                enc = self.tokenizer.__call__(
                    text,
                    truncation=True,
                    padding=False,
                    max_length=256,
                    return_tensors="pt",
                )
                if self.device_idx >= 0:
                    enc = {k: v.to(f"cuda:{self.device_idx}") for k, v in enc.items()}
                out = self.model(**enc)
                logits = out.logits
                pred_id = int(logits.argmax(dim=-1).item())
                label = id2label.get(pred_id, "neutral")
                return str(label).lower()

    return HFWrapper(model, tokenizer, device_idx)