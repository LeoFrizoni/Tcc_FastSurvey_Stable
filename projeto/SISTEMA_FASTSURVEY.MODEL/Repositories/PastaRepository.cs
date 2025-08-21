#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class PastaRepository : Repository<Pastas>, IPastaRepository
    {
        public PastaRepository(FastSurveyContext context) : base(context) { }

        public async Task<List<Pastas>> ListarPorLoginAsync(int loginId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                             .Where(p => p.Loginid == loginId)
                             .OrderBy(p => p.Nome)
                             .ToListAsync(ct);
        }
    }
}
