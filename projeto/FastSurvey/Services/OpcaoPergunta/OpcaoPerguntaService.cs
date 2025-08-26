// FASTSURVEY/Services/Opcoes/OpcaoPerguntaService.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Services.Result;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using OpcaoPergunta = SISTEMA_FASTSURVEY.MODEL.Models.OpcoesPergunta;
using PerguntaModel = SISTEMA_FASTSURVEY.MODEL.Models.Perguntas;

namespace FASTSURVEY.Services.Opcoes
{
    public class OpcaoPerguntaService : IOpcaoPerguntaService
    {
        private readonly FastSurveyContext _ctx;

        public OpcaoPerguntaService(FastSurveyContext ctx) => _ctx = ctx;

        // ---- Helpers ----
        private static OpcaoPerguntaResponse Map(OpcaoPergunta e) =>
            new()
            {
                OpcaoId = e.OpcaoId,
                PerguntaId = e.PerguntaId,
                Texto = e.Texto ?? string.Empty,
                Ordem = e.Ordem,
                Ativa = e.Ativa,
                Correta = e.Correta,
            };

        // ---- CREATE ----
        public async Task<ServiceResult<OpcaoPerguntaResponse>> CriarAsync(
            int perguntaId,
            OpcaoPerguntaRequest req,
            CancellationToken ct = default
        )
        {
            // valida pergunta
            var perguntaExiste = await _ctx.Set<PerguntaModel>()
                .AsNoTracking()
                .AnyAsync(p => p.PerguntaId == perguntaId, ct);

            if (!perguntaExiste)
                return ServiceResult<OpcaoPerguntaResponse>.Fail(
                    "PERGUNTA_NAO_ENCONTRADA",
                    "Pergunta não encontrada."
                );

            // calcula próxima ordem
            int proximaOrdem =
                (
                    await _ctx.Set<OpcaoPergunta>()
                        .Where(o => o.PerguntaId == perguntaId)
                        .MaxAsync(o => (int?)o.Ordem, ct) ?? 0
                ) + 1;

            var entity = new OpcaoPergunta
            {
                PerguntaId = perguntaId,
                Texto = (req.Texto ?? string.Empty).Trim(),
                Correta = req.Correta ?? false,
                Ordem = req.Ordem > 0 ? req.Ordem : proximaOrdem,
                Ativa = req.Ativa,
            };

            _ctx.Set<OpcaoPergunta>().Add(entity);
            await _ctx.SaveChangesAsync(ct);

            return ServiceResult<OpcaoPerguntaResponse>.Ok(Map(entity));
        }

        // ---- READ ----
        public async Task<ServiceResult<OpcaoPerguntaResponse>> ObterPorIdAsync(
            int opcaoId,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OpcaoId == opcaoId, ct);

            if (entity is null)
                return ServiceResult<OpcaoPerguntaResponse>.Fail(
                    "OPCAO_NAO_ENCONTRADA",
                    "Opção não encontrada."
                );

            return ServiceResult<OpcaoPerguntaResponse>.Ok(Map(entity));
        }

        public async Task<
            ServiceResult<IReadOnlyList<OpcaoPerguntaResponse>>
        > ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            var perguntaExiste = await _ctx.Set<PerguntaModel>()
                .AsNoTracking()
                .AnyAsync(p => p.PerguntaId == perguntaId, ct);

            if (!perguntaExiste)
                return ServiceResult<IReadOnlyList<OpcaoPerguntaResponse>>.Fail(
                    "PERGUNTA_NAO_ENCONTRADA",
                    "Pergunta não encontrada."
                );

            var list = await _ctx.Set<OpcaoPergunta>()
                .AsNoTracking()
                .Where(o => o.PerguntaId == perguntaId)
                .OrderBy(o => o.Ordem)
                .ThenBy(o => o.OpcaoId)
                .Select(o => Map(o))
                .ToListAsync(ct);

            return ServiceResult<IReadOnlyList<OpcaoPerguntaResponse>>.Ok(list);
        }

        // ---- UPDATE ----
        public async Task<ServiceResult<OpcaoPerguntaResponse>> AtualizarAsync(
            OpcaoPerguntaUpdateRequest req,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .FirstOrDefaultAsync(o => o.OpcaoId == req.OpcaoId, ct);

            if (entity is null)
                return ServiceResult<OpcaoPerguntaResponse>.Fail(
                    "OPCAO_NAO_ENCONTRADA",
                    "Opção não encontrada."
                );

            if (req.Texto != null)
                entity.Texto = req.Texto.Trim();
            if (req.Ordem != null)
                entity.Ordem = req.Ordem.Value;
            if (req.Ativa != null)
                entity.Ativa = req.Ativa.Value;
            if (req.Correta != null)
                entity.Correta = req.Correta.Value;

            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<OpcaoPerguntaResponse>.Ok(Map(entity));
        }

        // ---- DELETE (hard) ----
        public async Task<ServiceResult<bool>> RemoverAsync(
            int opcaoId,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx.Set<OpcaoPergunta>()
                .FirstOrDefaultAsync(o => o.OpcaoId == opcaoId, ct);
            if (entity is null)
                return ServiceResult<bool>.Fail("OPCAO_NAO_ENCONTRADA", "Opção não encontrada.");

            _ctx.Remove(entity);
            await _ctx.SaveChangesAsync(ct);
            return ServiceResult<bool>.Ok(true);
        }
    }
}
