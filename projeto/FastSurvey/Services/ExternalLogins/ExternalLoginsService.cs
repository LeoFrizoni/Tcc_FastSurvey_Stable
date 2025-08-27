using FASTSURVEY.Dtos.ExternalLogins;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.ExternalLogins
{
    public class ExternalLoginsService : IExternalLoginsService
    {
        private readonly IExternalLoginRepository _externalLoginRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ExternalLoginsService(IExternalLoginRepository externalLoginRepository, IUnitOfWork unitOfWork)
        {
            _externalLoginRepository = externalLoginRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ExternalLoginResponse?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default)
        {
            var externalLogin = await _externalLoginRepository.GetByProviderAsync(provider, providerUserId, ct);
            
            if (externalLogin == null) return null;

            return new ExternalLoginResponse
            {
                ExternalLoginId = externalLogin.ExternalLoginId,
                LoginId = externalLogin.LoginId,
                Provider = externalLogin.Provider,
                ProviderUserId = externalLogin.ProviderUserId,
                CriadoEm = externalLogin.CriadoEm
            };
        }

        public async Task<ExternalLoginResponse?> GetByLoginIdAsync(int loginId, CancellationToken ct = default)
        {
            var externalLogin = await _externalLoginRepository.GetByLoginIdAsync(loginId, ct);
            
            if (externalLogin == null) return null;

            return new ExternalLoginResponse
            {
                ExternalLoginId = externalLogin.ExternalLoginId,
                LoginId = externalLogin.LoginId,
                Provider = externalLogin.Provider,
                ProviderUserId = externalLogin.ProviderUserId,
                CriadoEm = externalLogin.CriadoEm
            };
        }

        public async Task<bool> ExistsAsync(string provider, string providerUserId, CancellationToken ct = default)
        {
            return await _externalLoginRepository.ExistsAsync(provider, providerUserId, ct);
        }

        public async Task<bool> ExistsForLoginAsync(int loginId, string provider, CancellationToken ct = default)
        {
            return await _externalLoginRepository.ExistsForLoginAsync(loginId, provider, ct);
        }

        public async Task<ExternalLoginResponse> CreateAsync(ExternalLoginRequest request, CancellationToken ct = default)
        {
            var externalLogin = new SISTEMA_FASTSURVEY.MODEL.Models.ExternalLogins
            {
                LoginId = request.LoginId,
                Provider = request.Provider,
                ProviderUserId = request.ProviderUserId,
                CriadoEm = DateTime.UtcNow
            };

            await _externalLoginRepository.AddAsync(externalLogin, ct);
            await _unitOfWork.CommitAsync(ct);

            return new ExternalLoginResponse
            {
                ExternalLoginId = externalLogin.ExternalLoginId,
                LoginId = externalLogin.LoginId,
                Provider = externalLogin.Provider,
                ProviderUserId = externalLogin.ProviderUserId,
                CriadoEm = externalLogin.CriadoEm
            };
        }

        public async Task<bool> DeleteAsync(int externalLoginId, CancellationToken ct = default)
        {
            var externalLogin = await _externalLoginRepository.GetByIdAsync(externalLoginId, ct);
            if (externalLogin == null) return false;

            await _externalLoginRepository.RemoveAsync(externalLogin, ct);
            await _unitOfWork.CommitAsync(ct);
            return true;
        }

        public async Task<List<ExternalLoginResponse>> GetAllByLoginIdAsync(int loginId, CancellationToken ct = default)
        {
            var externalLogins = await _externalLoginRepository.GetAllByLoginIdAsync(loginId, ct);
            
            return externalLogins.Select(e => new ExternalLoginResponse
            {
                ExternalLoginId = e.ExternalLoginId,
                LoginId = e.LoginId,
                Provider = e.Provider,
                ProviderUserId = e.ProviderUserId,
                CriadoEm = e.CriadoEm
            }).ToList();
        }
    }
}
