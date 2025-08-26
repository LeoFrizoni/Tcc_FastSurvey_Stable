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
    }
}
