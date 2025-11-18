-- Cria tabela de vínculo entre pesquisas e pastas
BEGIN;

CREATE TABLE IF NOT EXISTS public."PesquisasPastas" (
    "PesquisaId" integer NOT NULL,
    "PastaId" integer NOT NULL,
    "VinculadaEm" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "PK_PesquisasPastas" PRIMARY KEY ("PesquisaId"),
    CONSTRAINT "FK_PesquisasPastas_Pesquisas" FOREIGN KEY ("PesquisaId") REFERENCES public."Pesquisas" ("PesquisaId") MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT "FK_PesquisasPastas_Pastas" FOREIGN KEY ("PastaId") REFERENCES public."Pastas" ("PastaId") MATCH SIMPLE ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PesquisasPastas_PastaId" ON public."PesquisasPastas" ("PastaId");

-- Migra dados existentes, mantendo apenas o vínculo mais recente por pesquisa
INSERT INTO
    public."PesquisasPastas" (
        "PesquisaId",
        "PastaId",
        "VinculadaEm"
    )
SELECT DISTINCT
    ON (p."PesquisaId") p."PesquisaId",
    p."PastaId",
    COALESCE(
        p."DataAtualizacao",
        p."DataCriacao"
    ) AS "VinculadaEm"
FROM public."Pesquisas" AS p
WHERE
    p."PastaId" IS NOT NULL
ON CONFLICT ("PesquisaId") DO
UPDATE
SET
    "PastaId" = EXCLUDED."PastaId",
    "VinculadaEm" = EXCLUDED."VinculadaEm";

-- Caso deseje descontinuar a coluna antiga futuramente, remova o comentário abaixo
-- ALTER TABLE public."Pesquisas" DROP COLUMN IF EXISTS "PastaId";

COMMIT;