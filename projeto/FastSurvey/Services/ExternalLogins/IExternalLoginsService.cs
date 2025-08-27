using FASTSURVEY.Dtos.ExternalLogins;

namespace FASTSURVEY.Services.ExternalLogins
{
    public interface IExternalLoginsService
    {
        Task<ExternalLoginResponse?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
        Task<ExternalLoginResponse?> GetByLoginIdAsync(int loginId, CancellationToken ct = default);
        Task<bool> ExistsAsync(string provider, string providerUserId, CancellationToken ct = default);
        Task<bool> ExistsForLoginAsync(int loginId, string provider, CancellationToken ct = default);
        Task<ExternalLoginResponse> CreateAsync(ExternalLoginRequest request, CancellationToken ct = default);
        Task<bool> DeleteAsync(int externalLoginId, CancellationToken ct = default);
        Task<List<ExternalLoginResponse>> GetAllByLoginIdAsync(int loginId, CancellationToken ct = default);
    }
}
