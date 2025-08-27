#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class TokenRepository : Repository<Tokens>, ITokenRepository
    {
        public TokenRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<Tokens?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
            await _set.AsNoTracking().FirstOrDefaultAsync(x => x.TokenId == id, ct);

        public async Task<Tokens?> ObterPorCodigoAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;
            return await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token, ct);
            // Obs.: geralmente token é case-sensitive; por isso, evito ILIKE aqui.
        }

        public async Task<List<Tokens>> ListarAsync(
            bool apenasAtivos,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default
        )
        {
            var now = DateTime.UtcNow;

            var q = _set.AsNoTracking().OrderByDescending(x => x.DataRegistro).AsQueryable();

            if (apenasAtivos)
                q = q.Where(x => x.DataExpirado > now && x.UsadoEm == null);

            if (skip is int s && s > 0)
                q = q.Skip(s);
            if (take is int t && t > 0)
                q = q.Take(t);

            return await q.ToListAsync(ct);
        }

        // ===== Extensões úteis =====

        public async Task<Tokens?> ObterAtivoPorCodigoAsync(
            string token,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;
            var now = DateTime.UtcNow;

            return await _set.AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Token == token && x.UsadoEm == null && x.DataExpirado > now,
                    ct
                );
        }

        public async Task<List<Tokens>> ListarAtivosPorLoginAsync(
            int loginId,
            string? finalidade = null,
            int? skip = null,
            int? take = null,
            CancellationToken ct = default
        )
        {
            var now = DateTime.UtcNow;

            var q = _set.AsNoTracking()
                .Where(x => x.LoginId == loginId && x.UsadoEm == null && x.DataExpirado > now);

            if (!string.IsNullOrWhiteSpace(finalidade))
            {
                var f = finalidade.Trim();
                q = q.Where(x => x.Finalidade == f);
            }

            q = q.OrderByDescending(x => x.DataRegistro);

            if (skip is int s && s > 0)
                q = q.Skip(s);
            if (take is int t && t > 0)
                q = q.Take(t);

            return await q.ToListAsync(ct);
        }

        public async Task<bool> EhAtivoAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;
            var now = DateTime.UtcNow;

            return await _set.AsNoTracking()
                .AnyAsync(x => x.Token == token && x.UsadoEm == null && x.DataExpirado > now, ct);
        }

        public async Task<bool> MarcarComoUsadoAsync(
            string token,
            DateTime? quandoUtc = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var ent = await _set.FirstOrDefaultAsync(x => x.Token == token, ct);
            if (ent is null)
                return false;
            if (ent.UsadoEm != null)
                return true; // já estava usado; idempotente

            ent.UsadoEm = quandoUtc ?? DateTime.UtcNow;
            return true;
        }

        public async Task<bool> RevogarAsync(
            string token,
            DateTime? quandoUtc = null,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var ent = await _set.FirstOrDefaultAsync(x => x.Token == token, ct);
            if (ent is null)
                return false;

            var now = quandoUtc ?? DateTime.UtcNow;
            if (ent.DataExpirado <= now)
                return true; // já expirado; idempotente

            ent.DataExpirado = now;
            return true;
        }

        public async Task<int> LimparExpiradosAsync(
            bool incluirUsados = true,
            CancellationToken ct = default
        )
        {
            var now = DateTime.UtcNow;

            var q = _set.AsNoTracking().Where(x => x.DataExpirado <= now);

            if (!incluirUsados)
                q = q.Where(x => x.UsadoEm == null);

            var ids = await q.Select(x => x.TokenId).ToListAsync(ct);
            if (ids.Count == 0)
                return 0;

            var stubs = ids.Select(id => new Tokens { TokenId = id }).ToList();
            _set.RemoveRange(stubs);
            return ids.Count;
        }
    }
}
