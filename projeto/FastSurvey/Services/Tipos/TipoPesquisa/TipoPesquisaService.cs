#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Tipos
{
    public class TipoPesquisaService : ITipoPesquisaService
    {
        private readonly FastSurveyContext _ctx;
        public TipoPesquisaService(FastSurveyContext ctx) => _ctx = ctx;

        public async Task<List<Tipopesquisa>> ListarAsync(bool incluirDesabilitados = false, CancellationToken ct = default)
        {
            IQueryable<Tipopesquisa> q = _ctx.Set<Tipopesquisa>().AsNoTracking();

            if (!incluirDesabilitados)
                q = q.Where(x => !x.Desabilitado);

            // Ordena pelo nome, caindo para id quando nulo/vazio (caso Tipopesquisa1 venha nula do banco)
            return await q.OrderBy(x => string.IsNullOrWhiteSpace(x.Tipopesquisa1) ? null : x.Tipopesquisa1)
                          .ThenBy(x => x.Tipopesquisaid)
                          .ToListAsync(ct);
        }
    }
}
