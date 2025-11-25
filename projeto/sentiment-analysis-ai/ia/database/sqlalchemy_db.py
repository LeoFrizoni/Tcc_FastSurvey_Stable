"""Camada de acesso a dados usando SQLAlchemy para PostgreSQL.

Cria engine, sessão e o modelo ResponseResult para armazenar os resultados
da análise de sentimento.
"""

from sqlalchemy import create_engine, Column, Integer, String, Text, text as sql_text
from sqlalchemy.orm import declarative_base, sessionmaker
from config.settings import DATABASE_URL, RESULTS_TABLE_NAME

Base = declarative_base()


class ResponseResult(Base):
    __tablename__ = RESULTS_TABLE_NAME
    id = Column(Integer, primary_key=True, autoincrement=True)
    text = Column(Text, nullable=False)
    sentiment = Column(String(32), nullable=False)
    category = Column(String(32), nullable=False, default="neutral")

    def __repr__(self) -> str:
        return f"<ResponseResult(id={self.id}, sentiment={self.sentiment})>"


def get_engine():
    return create_engine(DATABASE_URL, echo=False, future=True)


def get_session_factory():
    engine = get_engine()
    Base.metadata.create_all(engine)
    ensure_schema(engine)
    return sessionmaker(bind=engine, autoflush=False, autocommit=False, future=True)


def ensure_schema(engine=None):
    """Garante que a tabela possua a coluna 'category'.

    Em PostgreSQL, usa ALTER TABLE IF NOT EXISTS para adicionar a coluna.
    """
    eng = engine or get_engine()
    ddl = f"ALTER TABLE {RESULTS_TABLE_NAME} ADD COLUMN IF NOT EXISTS category VARCHAR(32) NOT NULL DEFAULT 'neutral';"
    with eng.begin() as conn:
        conn.execute(sql_text(ddl))
