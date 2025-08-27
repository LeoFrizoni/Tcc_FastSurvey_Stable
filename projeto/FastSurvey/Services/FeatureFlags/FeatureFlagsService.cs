using FASTSURVEY.Dtos.FeatureFlags;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;

namespace FASTSURVEY.Services.FeatureFlags
{
    public class FeatureFlagsService : IFeatureFlagsService
    {
        private readonly IFeatureFlagsRepository _featureFlagsRepository;

        public FeatureFlagsService(IFeatureFlagsRepository featureFlagsRepository)
        {
            _featureFlagsRepository = featureFlagsRepository;
        }

        public async Task<bool> IsFeatureEnabledAsync(int loginId, string flag, CancellationToken ct = default)
        {
            return await _featureFlagsRepository.IsFeatureEnabledAsync(loginId, flag, ct);
        }

        public async Task<FeatureFlagResponse> SetFeatureFlagAsync(FeatureFlagRequest request, CancellationToken ct = default)
        {
            var featureFlag = await _featureFlagsRepository.SetFeatureFlagAsync(
                request.LoginId, 
                request.Flag, 
                request.Ativa, 
                ct
            );

            return new FeatureFlagResponse
            {
                LoginId = featureFlag.LoginId,
                Flag = featureFlag.Flag,
                Ativa = featureFlag.Ativa
            };
        }

        public async Task<bool> RemoveFeatureFlagAsync(int loginId, string flag, CancellationToken ct = default)
        {
            return await _featureFlagsRepository.RemoveFeatureFlagAsync(loginId, flag, ct);
        }

        public async Task<List<FeatureFlagResponse>> ListarPorLoginAsync(int loginId, CancellationToken ct = default)
        {
            var featureFlags = await _featureFlagsRepository.ListarPorLoginAsync(loginId, ct);
            
            return featureFlags.Select(f => new FeatureFlagResponse
            {
                LoginId = f.LoginId,
                Flag = f.Flag,
                Ativa = f.Ativa
            }).ToList();
        }

        public async Task<List<FeatureFlagResponse>> ListarPorFlagAsync(string flag, CancellationToken ct = default)
        {
            var featureFlags = await _featureFlagsRepository.ListarPorFlagAsync(flag, ct);
            
            return featureFlags.Select(f => new FeatureFlagResponse
            {
                LoginId = f.LoginId,
                Flag = f.Flag,
                Ativa = f.Ativa
            }).ToList();
        }
    }
}
