-- Script para remover a constraint de unicidade problemática em OpcoesPergunta
-- Esta constraint impede que uma pergunta tenha opções com textos duplicados,
-- mas isso é uma limitação desnecessária (ex: múltiplas opções "Outro", "N/A", etc.)

-- Verificar se a constraint existe
SELECT
    constraint_name,
    constraint_type
FROM information_schema.table_constraints
WHERE
    table_name = 'OpcoesPergunta'
    AND constraint_name = 'ux_opcoespergunta_perguntaid_texto';

-- Remover a constraint de unicidade
ALTER TABLE "OpcoesPergunta"
DROP CONSTRAINT IF EXISTS ux_opcoespergunta_perguntaid_texto;

-- Verificar se foi removida
SELECT
    constraint_name,
    constraint_type
FROM information_schema.table_constraints
WHERE
    table_name = 'OpcoesPergunta';

-- NOTA: Se você realmente precisa evitar duplicatas exatas,
-- isso deve ser validado na camada de aplicação (backend),
-- não como constraint do banco de dados.