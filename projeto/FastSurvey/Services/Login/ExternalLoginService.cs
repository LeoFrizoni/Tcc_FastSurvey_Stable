#nullable enable
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Auth;
using FASTSURVEY.Dtos.Login;

namespace FASTSURVEY.Services.Login
{
    public interface IExternalLoginService
    {
        Task<LoginResponse> GoogleAsync(ExternalLoginRequest req, CancellationToken ct = default);
    }

    public sealed class ExternalLoginService : IExternalLoginService
    {
        private readonly ILoginService _login;
        public ExternalLoginService(ILoginService login) => _login = login;

        public Task<LoginResponse> GoogleAsync(ExternalLoginRequest req, CancellationToken ct = default)
            => _login.LoginGoogleAsync(req, ct);
    }
}
