import os
import unittest
from datetime import datetime, timedelta

from sqlalchemy import JSON, Column, DateTime, Integer, MetaData, Table, create_engine, func, select, update

# Evita conexoes reais caso a variavel ja esteja configurada no ambiente de desenvolvimento.
os.environ.setdefault("SENTIMENT_DB_URL", "sqlite:///:memory:")

from ia.database.analysis_store import AnalysisStore  # noqa: E402  pylint: disable=C0413


class AnalysisStoreTests(unittest.TestCase):
    def setUp(self):
        self.engine = create_engine("sqlite:///:memory:", future=True)
        metadata = MetaData()
        self.table = Table(
            "Analises",
            metadata,
            Column("id", Integer, primary_key=True, autoincrement=True),
            Column("resposta_id", Integer, nullable=False, unique=True),
            Column("analytics", JSON, nullable=False),
            Column("keywords", JSON, nullable=False),
            Column("created_at", DateTime, nullable=False, server_default=func.now()),
        )
        metadata.create_all(self.engine)
        self.store = AnalysisStore(engine=self.engine)

    def test_upsert_inserts_new_row(self):
        analytics = {"sentiment": "positive", "wordCount": 10}
        keywords = [("otimo", 2), ("app", 1)]

        self.store.upsert(1, analytics, keywords)

        with self.engine.connect() as conn:
            row = conn.execute(select(self.table)).mappings().one()

        self.assertEqual(row["resposta_id"], 1)
        self.assertEqual(row["analytics"]["sentiment"], "positive")
        self.assertEqual(row["keywords"], [["otimo", 2], ["app", 1]])

    def test_upsert_updates_existing_row(self):
        self.store.upsert(2, {"sentiment": "neutral"}, [("ok", 1)])
        self.store.upsert(2, {"sentiment": "negative", "charCount": 20}, [("bug", 3)])

        with self.engine.connect() as conn:
            row = conn.execute(select(self.table)).mappings().one()

        self.assertEqual(row["resposta_id"], 2)
        self.assertEqual(row["analytics"], {"sentiment": "negative", "charCount": 20})
        self.assertEqual(row["keywords"], [["bug", 3]])

    def test_list_last_respects_limit_and_order(self):
        base_time = datetime(2024, 1, 1, 12, 0, 0)
        for idx in range(3):
            resposta_id = 10 + idx
            self.store.upsert(resposta_id, {"sentiment": "neutral", "idx": idx}, [("palavra", 1)])
            self._override_created_at(resposta_id, base_time + timedelta(minutes=idx))

        latest = list(self.store.list_last(limit=2))

        self.assertEqual([item["resposta_id"] for item in latest], [12, 11])
        self.assertEqual(len(latest), 2)

    def _override_created_at(self, resposta_id: int, timestamp):
        with self.engine.begin() as conn:
            conn.execute(
                update(self.table)
                .where(self.table.c.resposta_id == resposta_id)
                .values(created_at=timestamp)
            )


if __name__ == "__main__":
    unittest.main()
