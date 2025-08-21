#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class OpcaoPerguntaRepository : Repository<Opcoespergunta>, IOpcaoPerguntaRepository
    {
        public OpcaoPerguntaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Opcoespergunta>> ListarPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Where(o => o.Perguntaid == perguntaId)
                             .OrderBy(o => o.Ordem)
                             .ToListAsync(ct);
        }

        public async Task<List<Opcoespergunta>> ListarAtivasPorPerguntaAsync(int perguntaId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Where(o => o.Perguntaid == perguntaId && o.Ativa)
                             .OrderBy(o => o.Ordem)
                             .ToListAsync(ct);
        }
    }
}
