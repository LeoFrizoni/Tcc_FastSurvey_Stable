#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TokenRepository : Repository<Tokens>, ITokenRepository
    {
        public TokenRepository(FastSurveyContext context) : base(context) { }

        public async Task<Tokens?> ObterPorIdAsync(int id, CancellationToken ct = default)
            => await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Tokenid == id, ct);

        public async Task<Tokens?> ObterPorCodigoAsync(string token, CancellationToken ct = default)
            => await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token, ct);

        public async Task<List<Tokens>> ListarAsync(bool apenasAtivos, int? skip = null, int? take = null, CancellationToken ct = default)
        {
            var q = _set.AsNoTracking().OrderByDescending(x => x.Dataregistro).AsQueryable();
            if (apenasAtivos) q = q.Where(x => x.Dataexpirado > DateTime.UtcNow);
            if (skip is int s) q = q.Skip(s);
            if (take is int t) q = q.Take(t);
            return await q.ToListAsync(ct);
        }
    }
}
