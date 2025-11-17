-- Script para corrigir o problema do trigger dataatualizacao

-- Opção 1: Remover o trigger (RECOMENDADO - EF Core já gerencia DataAtualizacao)
DROP TRIGGER IF EXISTS trg_set_dataatualizacao ON "Pesquisas";

DROP FUNCTION IF EXISTS trg_set_dataatualizacao ();

-- Opção 2: Corrigir o trigger para usar o nome correto da coluna
-- Se você quiser manter o trigger, descomente e execute:

/*
CREATE OR REPLACE FUNCTION trg_set_dataatualizacao()
RETURNS TRIGGER AS $$
BEGIN
NEW."DataAtualizacao" := CURRENT_TIMESTAMP;  -- Usa PascalCase
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_set_dataatualizacao
BEFORE UPDATE ON "Pesquisas"
FOR EACH ROW
EXECUTE FUNCTION trg_set_dataatualizacao();
*/

-- Verificar se o trigger foi removido
SELECT *
FROM information_schema.triggers
WHERE
    event_object_table = 'Pesquisas'
    AND trigger_name = 'trg_set_dataatualizacao';