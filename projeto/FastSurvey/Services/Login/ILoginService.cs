#nullable enable
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login;

namespace FASTSURVEY.Services.Login
{
    public interface ILoginService
    {
        Task<LoginResponse> AutenticarAsync(LoginRequest req, CancellationToken ct = default);
        Task<CadastrarLoginResponse> CadastrarAsync(CadastrarLoginRequest req, CancellationToken ct = default);

        Task<bool> SolicitarResetSenhaAsync(ForgotPasswordRequest req, CancellationToken ct = default);
        Task<bool> ResetarSenhaAsync(ResetPasswordRequest req, CancellationToken ct = default);

        Task<bool> EnviarConfirmacaoEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ConfirmarEmailAsync(VerifyEmailRequest req, CancellationToken ct = default);

        Task<LoginResponse> LoginGoogleAsync(ExternalLoginRequest req, CancellationToken ct = default);
        
        Task<PerfilResponse?> ObterPerfilAsync(int loginId, CancellationToken ct = default);
        Task<bool> AtualizarPerfilAsync(int loginId, string? email, string? senha, CancellationToken ct = default);
    }
}
