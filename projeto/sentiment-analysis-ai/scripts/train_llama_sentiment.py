r"""
Treinamento de classificação de sentimento com LLaMA/TinyLlama usando PEFT (LoRA).

Requer:
- transformers
- datasets
- peft
- accelerate
- torch (com CUDA recomendado)

Saída:
- Adaptador LoRA salvo em diretório destino (ex.: models/llama-sentiment-adapter)

Uso (PowerShell):
  $env:SENTIMENT_HF_BASE_MODEL_ID="TinyLlama/TinyLlama-1.1B-Chat-v1.0"
    python .\scripts\train_llama_sentiment.py --dataset data/sentiment_dataset.csv --output_dir models/llama-sentiment-adapter --epochs 2
"""

import argparse
from dataclasses import dataclass
from typing import List, Any

import pandas as pd
from datasets import Dataset
from transformers import (
    AutoTokenizer,
    AutoModelForSequenceClassification,
    TrainingArguments,
    Trainer,
)
from peft import LoraConfig, get_peft_model


LABEL2ID = {"negative": 0, "neutral": 1, "positive": 2}
ID2LABEL = {v: k for k, v in LABEL2ID.items()}


def load_csv_dataset(path: str) -> Dataset:
    df = pd.read_csv(path)
    # Espera colunas: text,label em {negative,neutral,positive}
    df = df.dropna(subset=["text", "label"]).copy()
    df["label_id"] = df["label"].map(LABEL2ID)
    return Dataset.from_pandas(df[["text", "label_id"]], preserve_index=False)


@dataclass
class DataCollator:
    tokenizer: Any
    max_length: int = 256

    def __call__(self, examples: List[dict]):
        texts = [ex["text"] for ex in examples]
        labels = [int(ex["label_id"]) for ex in examples]
        enc = self.tokenizer.__call__(
            texts,
            truncation=True,
            padding=True,
            max_length=self.max_length,
            return_tensors="pt",
        )
        enc["labels"] = __import__("torch").tensor(labels)
        return enc


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--dataset", required=True, help="CSV com colunas text,label")
    parser.add_argument("--output_dir", required=True, help="Diretório de saída do adaptador LoRA")
    parser.add_argument("--base_model", default=None, help="ID do modelo base HF (se vazio, usa env SENTIMENT_HF_BASE_MODEL_ID)")
    parser.add_argument("--epochs", type=int, default=2)
    parser.add_argument("--batch_size", type=int, default=4)
    parser.add_argument("--lr", type=float, default=2e-4)
    args = parser.parse_args()

    import os
    base_model_id = args.base_model or os.getenv("SENTIMENT_HF_BASE_MODEL_ID")
    if not base_model_id:
        raise ValueError("Defina --base_model ou a variável SENTIMENT_HF_BASE_MODEL_ID")

    tokenizer = AutoTokenizer.from_pretrained(base_model_id, use_fast=True)
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token
    
    model = AutoModelForSequenceClassification.from_pretrained(
        base_model_id,
        num_labels=3,
        id2label=ID2LABEL,
        label2id=LABEL2ID,
    )
    
    # Configurar pad_token_id no modelo também
    if model.config.pad_token_id is None:
        model.config.pad_token_id = tokenizer.pad_token_id

    peft_cfg = LoraConfig(
        r=8,
        lora_alpha=16,
        target_modules=["q_proj", "v_proj"],
        lora_dropout=0.05,
        bias="none",
        task_type="SEQ_CLS",
    )
    model = get_peft_model(model, peft_cfg)

    ds = load_csv_dataset(args.dataset)
    # split simples
    split = ds.train_test_split(test_size=0.1, seed=42)
    train_ds, eval_ds = split["train"], split["test"]

    collator = DataCollator(tokenizer)

    training_args = TrainingArguments(
        output_dir=args.output_dir,
        per_device_train_batch_size=args.batch_size,
        per_device_eval_batch_size=args.batch_size,
        num_train_epochs=args.epochs,
        learning_rate=args.lr,
        logging_steps=10,
        remove_unused_columns=False,
    )

    def compute_metrics(eval_pred):
        from sklearn.metrics import accuracy_score, f1_score
        logits, labels = eval_pred
        import numpy as np
        preds = logits.argmax(axis=-1)
        return {
            "accuracy": accuracy_score(labels, preds),
            "f1_macro": f1_score(labels, preds, average="macro"),
        }

    trainer = Trainer(
        model=model,
        args=training_args,
        train_dataset=train_ds,
        eval_dataset=eval_ds,
        data_collator=collator,
        compute_metrics=compute_metrics,
    )

    trainer.train()
    # Salva apenas o adaptador LoRA (PEFT)
    model.save_pretrained(args.output_dir)
    print(f"Adaptador LoRA salvo em: {args.output_dir}")


if __name__ == "__main__":
    main()
