#nullable enable
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

// ===== ALIAS para casar com a Model do scaffold =====
using OpcaoPergunta = SISTEMA_FASTSURVEY.MODEL.Models.Opcoespergunta; // Tabela: opcoespergunta
using PerguntaModel = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace FASTSURVEY.Services.Opcoes
{
    public class OpcaoPerguntaService : IOpcaoPerguntaService
    {
        private readonly FastSurveyContext _ctx;
        public OpcaoPerguntaService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<ServiceResult<OpcaoPergunta>> CriarAsync(
            int perguntaId,
            string texto,
            bool? correta = null,
            int? ordem = null,
            bool? ativa = null,
            CancellationToken ct = default)
        {
            // Confere existência da pergunta
            var perguntaExiste = await _ctx.Set<PerguntaModel>()
                .AsNoTracking()
                .AnyAsync(p => p.Perguntaid == perguntaId, ct);

            if (!perguntaExiste)
                return ServiceResult<OpcaoPergunta>.Fail("PERGUNTA_NAO_ENCONTRADA", "Pergunta não encontrada.");

            // Se ordem não for informada, usa a próxima ordem disponível
            int proximaOrdem = (await _ctx.Set<OpcaoPergunta>()
                                    .Where(o => o.Perguntaid == perguntaId)
                                    .MaxAsync(o => (int?)o.Ordem, ct) ?? 0) + 1;

            var entity = new OpcaoPergunta
            {
                Perguntaid = perguntaId,
                Texto = (texto ?? string.Empty).Trim(),
                Correta = correta ?? false,
                Ordem = ordem ?? proximaOrdem,
                Ativa = ativa ?? true
            };

            _ctx.Set<OpcaoPergunta>().Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return ServiceResult<OpcaoPergunta>.Ok(entity);
        }

        public async Task<ServiceResult<OpcaoPergunta>> ObterPorIdAsync(int opcaoId, CancellationToken ct = default)
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Opcaoid == opcaoId, ct);

            if (entity is null)
                return ServiceResult<OpcaoPergunta>.Fail("OPCAO_NAO_ENCONTRADA", "Opção não encontrada.");

            return ServiceResult<OpcaoPergunta>.Ok(entity);
        }

        public async Task<ServiceResult<List<OpcaoPergunta>>> ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            // Valida pergunta (opcional; mantém mensagem mais clara)
            var perguntaExiste = await _ctx.Set<PerguntaModel>()
                .AsNoTracking()
                .AnyAsync(p => p.Perguntaid == perguntaId, ct);

            if (!perguntaExiste)
                return ServiceResult<List<OpcaoPergunta>>.Fail("PERGUNTA_NAO_ENCONTRADA", "Pergunta não encontrada.");

            var list = await _ctx.Set<OpcaoPergunta>()
                .AsNoTracking()
                .Where(o => o.Perguntaid == perguntaId)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.Opcaoid) // desempate estável
                .ToListAsync(ct);

            return ServiceResult<List<OpcaoPergunta>>.Ok(list);
        }

        public async Task<ServiceResult<OpcaoPergunta>> AtualizarAsync(
            int opcaoId,
            Action<OpcaoPergunta> applyUpdates,
            CancellationToken ct = default)
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .FirstOrDefaultAsync(o => o.Opcaoid == opcaoId, ct);

            if (entity is null)
                return ServiceResult<OpcaoPergunta>.Fail("OPCAO_NAO_ENCONTRADA", "Opção não encontrada.");

            applyUpdates?.Invoke(entity);
            await _ctx.SaveChangesAsync(ct);

            return ServiceResult<OpcaoPergunta>.Ok(entity);
        }

        public async Task<ServiceResult<bool>> RemoverAsync(int opcaoId, CancellationToken ct = default)
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .FirstOrDefaultAsync(o => o.Opcaoid == opcaoId, ct);

            if (entity is null)
                return ServiceResult<bool>.Fail("OPCAO_NAO_ENCONTRADA", "Opção não encontrada.");

            // Hard delete (se quiser soft delete, troque para: entity.Ativa = false)
            _ctx.Set<OpcaoPergunta>().Remove(entity);
            await _ctx.SaveChangesAsync(ct);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
