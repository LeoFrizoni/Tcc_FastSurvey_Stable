#nullable enable
using System.Collections.Generic; // <--- necessário para IEnumerable<>
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login;

namespace FASTSURVEY.Services.Login
{
    /// <summary>
    /// Interface para serviços de autenticação e gerenciamento de usuários
    /// </summary>
    public interface ILoginService
    {
        // =========================================================
        //                    AUTENTICAÇÃO
        // =========================================================
        Task<LoginResponse> AutenticarAsync(LoginRequest request, CancellationToken ct = default);
        Task<LoginResponse> LoginExternoAsync(
            ExternalLoginRequest request,
            CancellationToken ct = default
        );
        Task<CadastrarLoginResponse> CadastrarAsync(
            CadastrarLoginRequest request,
            CancellationToken ct = default
        );

        // =========================================================
        //                    RESET DE SENHA
        // =========================================================
        Task<bool> SolicitarResetSenhaAsync(
            ForgotPasswordRequest request,
            CancellationToken ct = default
        );
        Task<bool> ResetarSenhaAsync(ResetPasswordRequest request, CancellationToken ct = default);

        // =========================================================
        //                    CONFIRMAÇÃO DE EMAIL
        // =========================================================
        Task<bool> EnviarConfirmacaoEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ConfirmarEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);

        // =========================================================
        //                    PERFIL DO USUÁRIO
        // =========================================================
        Task<PerfilResponse?> ObterPerfilAsync(int loginId, CancellationToken ct = default);
        Task<bool> AtualizarPerfilAsync(
            int loginId,
            AtualizarPerfilRequest request,
            CancellationToken ct = default
        );
        Task<bool> AtualizarNomeAsync(
            int loginId,
            AtualizarNomeRequest request,
            CancellationToken ct = default
        );

        // =========================================================
        //                    VALIDAÇÕES
        // =========================================================
        Task<bool> EmailExisteAsync(string email, CancellationToken ct = default);
        Task<bool> UsuarioExisteAsync(string usuario, CancellationToken ct = default);

        // =========================================================
        //                    FUNCIONALIDADES ADMIN
        // =========================================================
        Task<IEnumerable<PerfilResponse>> ListarTodosUsuariosAsync(CancellationToken ct = default);
        Task<bool> ExcluirUsuarioAsync(int loginId, CancellationToken ct = default);
        Task<bool> AtivarUsuarioAsync(int loginId, CancellationToken ct = default);
        Task<bool> DesativarUsuarioAsync(int loginId, CancellationToken ct = default);
    }
}
