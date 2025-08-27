using FASTSURVEY.Dtos.FeatureFlags;

namespace FASTSURVEY.Services.FeatureFlags
{
    public interface IFeatureFlagsService
    {
        Task<bool> IsFeatureEnabledAsync(int loginId, string flag, CancellationToken ct = default);
        Task<FeatureFlagResponse> SetFeatureFlagAsync(FeatureFlagRequest request, CancellationToken ct = default);
        Task<bool> RemoveFeatureFlagAsync(int loginId, string flag, CancellationToken ct = default);
        Task<List<FeatureFlagResponse>> ListarPorLoginAsync(int loginId, CancellationToken ct = default);
        Task<List<FeatureFlagResponse>> ListarPorFlagAsync(string flag, CancellationToken ct = default);
    }
}
