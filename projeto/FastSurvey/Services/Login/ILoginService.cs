// Services/Interfaces/IAuthService.cs

// Services/Interfaces/IAuthService.cs
using FASTSURVEY.Dtos.Auth;

namespace FASTSURVEY.Services.Login
{
    public interface ILoginService
    {
        Task<LoginResponse> AutenticarAsync(LoginRequest req, CancellationToken ct);
        Task<CadastrarLoginResponse> CadastrarAsync(CadastrarLoginRequest req, CancellationToken ct);
        string GerarJwtToken(int loginId, int tipoUsuarioId, string usuario);
    }
}
