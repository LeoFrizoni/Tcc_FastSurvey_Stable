"""Persistência das análises semânticas na tabela Analises do FastSurvey.

Este módulo expõe um repositório simples baseado em SQLAlchemy que realiza
upsert (insert/update) para registrar o resultado da IA vinculada a cada resposta
(textual) criada no sistema.
"""
from __future__ import annotations

from typing import Any, Iterable, List, Sequence, Tuple

from sqlalchemy import MetaData, Table, create_engine, func, select, update, insert
from sqlalchemy.engine import Engine
from sqlalchemy.exc import NoSuchTableError
from sqlalchemy.orm import sessionmaker

from config.settings import DATABASE_URL


class AnalysisStore:
    """Persistência para a tabela Analises."""

    def __init__(self, engine: Engine | None = None):
        self._engine = engine or create_engine(DATABASE_URL, echo=False, future=True)
        self._session_factory = sessionmaker(
            bind=self._engine,
            autoflush=False,
            autocommit=False,
            future=True,
        )
        metadata = MetaData()
        try:
            self._table = Table("Analises", metadata, autoload_with=self._engine)
        except NoSuchTableError as exc:  # pragma: no cover - só acontece em ambientes sem migração
            raise RuntimeError(
                "Tabela 'Analises' não encontrada. Execute as migrações do FastSurvey antes de usar a IA."
            ) from exc

    def upsert(self, resposta_id: int, analytics: dict[str, Any], keywords: Sequence[Tuple[str, int]]):
        """Insere ou atualiza a análise para a resposta informada."""
        keywords_payload: List[List[Any]] = [
            [str(word), int(freq)] for word, freq in keywords if word
        ]

        with self._session_factory() as session:
            existing = session.execute(
                select(self._table.c.id).where(self._table.c.resposta_id == resposta_id)
            ).scalar_one_or_none()

            if existing:
                session.execute(
                    update(self._table)
                    .where(self._table.c.resposta_id == resposta_id)
                    .values(
                        analytics=analytics,
                        keywords=keywords_payload,
                        created_at=func.now(),
                    )
                )
            else:
                session.execute(
                    insert(self._table).values(
                        id=resposta_id,
                        resposta_id=resposta_id,
                        analytics=analytics,
                        keywords=keywords_payload,
                    )
                )
            session.commit()

    def list_last(self, limit: int = 10) -> Iterable[dict[str, Any]]:
        """Retorna as últimas análises persistidas (usado em diagnósticos)."""
        with self._session_factory() as session:
            rows = session.execute(
                select(self._table)
                .order_by(self._table.c.created_at.desc())
                .limit(max(1, limit))
            ).mappings()
            for row in rows:
                yield dict(row)
