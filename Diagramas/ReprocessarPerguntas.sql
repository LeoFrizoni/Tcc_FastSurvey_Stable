-- Script para processar perguntas de pesquisas existentes
-- Este script extrai perguntas do TemplateJson e cria registros na tabela Perguntas

-- Primeiro, vamos verificar a pesquisa 84
SELECT pesquisaid, titulo, templatejson
FROM pesquisas
WHERE
    pesquisaid = 84;

-- Para processar manualmente a pesquisa 84, você pode executar via API:
-- POST /api/Pesquisas/84/reprocessar-perguntas
-- Ou usar o método ParseTemplateAndCreatePerguntas do PesquisaService

-- Alternativa: Criar um endpoint temporário no controller para reprocessar pesquisas existentes