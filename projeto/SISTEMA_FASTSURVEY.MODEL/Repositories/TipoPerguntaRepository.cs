#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TipoPerguntaRepository : Repository<Tipopergunta>, ITipoPerguntaRepository
    {
        public TipoPerguntaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Tipopergunta>> ListarAtivosAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Where(tp => !tp.Desabilitado)
                             .OrderBy(tp => tp.Tipopergunta1 ?? string.Empty)
                             .ToListAsync(ct);
        }
    }
}
