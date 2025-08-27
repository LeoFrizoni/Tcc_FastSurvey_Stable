// FASTSURVEY/Services/Tipos/TipoPesquisaService.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tipos
{
    public class TipoPesquisaService : ITipoPesquisaService
    {
        private readonly FastSurveyContext _ctx;

        public TipoPesquisaService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<IReadOnlyList<TipoPesquisaCatalogDto>> ListarAsync(
            bool incluirDesabilitados = false,
            CancellationToken ct = default
        )
        {
            var q = _ctx.Set<TipoPesquisa>().AsNoTracking();
            if (!incluirDesabilitados)
                q = q.Where(x => !x.Desabilitado);

            var list = await q.OrderBy(x =>
                    string.IsNullOrWhiteSpace(x.TipoPesquisa1) ? null : x.TipoPesquisa1
                )
                .ThenBy(x => x.TipoPesquisaId)
                .Select(x => new TipoPesquisaCatalogDto
                {
                    TipoPesquisaId = x.TipoPesquisaId,
                    TipoPesquisa = x.TipoPesquisa1 ?? string.Empty,
                    Desabilitado = x.Desabilitado,
                })
                .ToListAsync(ct);

            return list;
        }

        public async Task<TipoPesquisaCatalogDto> CadastrarAsync(string tipoPesquisa, CancellationToken ct = default)
        {
            var novo = new SISTEMA_FASTSURVEY.MODEL.Models.TipoPesquisa
            {
                TipoPesquisa1 = tipoPesquisa.Trim(),
                Desabilitado = false
            };

            _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoPesquisa>().Add(novo);
            await _ctx.SaveChangesAsync(ct);

            return new TipoPesquisaCatalogDto
            {
                TipoPesquisaId = novo.TipoPesquisaId,
                TipoPesquisa = novo.TipoPesquisa1,
                Desabilitado = novo.Desabilitado
            };
        }

        public async Task<bool> AlterarAsync(int id, string tipoPesquisa, CancellationToken ct = default)
        {
            var existente = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoPesquisa>()
                .FirstOrDefaultAsync(x => x.TipoPesquisaId == id, ct);

            if (existente == null)
                return false;

            existente.TipoPesquisa1 = tipoPesquisa.Trim();
            await _ctx.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
        {
            var existente = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoPesquisa>()
                .FirstOrDefaultAsync(x => x.TipoPesquisaId == id, ct);

            if (existente == null)
                return false;

            // Verificar se há pesquisas usando este tipo
            var pesquisasComTipo = await _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.Pesquisas>()
                .AnyAsync(x => x.TipoPesquisaId == id, ct);

            if (pesquisasComTipo)
                throw new InvalidOperationException("Não é possível excluir um tipo de pesquisa que está sendo usado.");

            _ctx.Set<SISTEMA_FASTSURVEY.MODEL.Models.TipoPesquisa>().Remove(existente);
            await _ctx.SaveChangesAsync(ct);

            return true;
        }
    }
}
