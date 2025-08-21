#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoPesquisaRepository : Repository<Tipopesquisa>, ITipoPesquisaRepository
    {
        public TipoPesquisaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Tipopesquisa>> ListarAtivosAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Where(tp => !tp.Desabilitado)
                             .OrderBy(tp => tp.Tipopesquisaid)
                             .ToListAsync(ct);
        }
    }
}
