#nullable enable
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class FeatureFlagsRepository : Repository<FeatureFlags>, IFeatureFlagsRepository
    {
        public FeatureFlagsRepository(FastSurveyContext context)
            : base(context) { }

        public async Task<bool> IsFeatureEnabledAsync(int loginId, string flag, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(flag))
                return false;

            return await _set.AsNoTracking()
                .AnyAsync(f => f.LoginId == loginId && f.Flag == flag && f.Ativa, ct);
        }

        public async Task<FeatureFlags> SetFeatureFlagAsync(
            int loginId, 
            string flag, 
            bool ativa, 
            CancellationToken ct = default
        )
        {
            var existing = await _set.FirstOrDefaultAsync(
                f => f.LoginId == loginId && f.Flag == flag, ct);

            if (existing != null)
            {
                existing.Ativa = ativa;
                _set.Update(existing);
                return existing;
            }
            else
            {
                var featureFlag = new FeatureFlags
                {
                    LoginId = loginId,
                    Flag = flag,
                    Ativa = ativa
                };

                await _set.AddAsync(featureFlag, ct);
                return featureFlag;
            }
        }

        public async Task<bool> RemoveFeatureFlagAsync(int loginId, string flag, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(flag))
                return false;

            var affected = await _set.Where(f => f.LoginId == loginId && f.Flag == flag)
                .ExecuteDeleteAsync(ct);

            return affected > 0;
        }

        public async Task<List<FeatureFlags>> ListarPorLoginAsync(int loginId, CancellationToken ct = default)
        {
            return await _set.AsNoTracking()
                .Where(f => f.LoginId == loginId)
                .OrderBy(f => f.Flag)
                .ToListAsync(ct);
        }

        public async Task<List<FeatureFlags>> ListarPorFlagAsync(string flag, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(flag))
                return new List<FeatureFlags>();

            return await _set.AsNoTracking()
                .Where(f => f.Flag == flag && f.Ativa)
                .OrderBy(f => f.LoginId)
                .ToListAsync(ct);
        }
    }
}
