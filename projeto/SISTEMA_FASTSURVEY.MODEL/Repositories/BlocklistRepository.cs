#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class BlocklistRepository : Repository<Blocklist>, IBlocklistRepository
    {
        public BlocklistRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<bool> IsBlockedAsync(string ipAddress, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return await _set.AsNoTracking()
                .AnyAsync(b => b.IPAddress == ipAddress, ct);
        }

        public async Task<Blocklist> AddToBlocklistAsync(
            string ipAddress, 
            string motivo, 
            string? userAgent = null, 
            CancellationToken ct = default
        )
        {
            var blocklist = new Blocklist
            {
                IPAddress = ipAddress,
                Motivo = motivo,
                UserAgent = userAgent ?? string.Empty,
                CriadoEm = DateTime.UtcNow
            };

            await _set.AddAsync(blocklist, ct);
            return blocklist;
        }

        public async Task<bool> RemoveFromBlocklistAsync(string ipAddress, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            var affected = await _set.Where(b => b.IPAddress == ipAddress)
                .ExecuteDeleteAsync(ct);

            return affected > 0;
        }

        public async Task<List<Blocklist>> ListarTodosAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .OrderByDescending(b => b.CriadoEm)
                .ToListAsync(ct);
        }

        public async Task<int> LimparAntigasAsync(DateTime antesDe, CancellationToken ct = default)
        {
            return await _set.Where(b => b.CriadoEm < antesDe)
                .ExecuteDeleteAsync(ct);
        }
    }
}
