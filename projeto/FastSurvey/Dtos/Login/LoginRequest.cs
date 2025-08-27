using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Login
{
    // =========================================================
    //                    DTOs DE AUTENTICAÇÃO
    // =========================================================

    /// <summary>
    /// DTO para login tradicional (email/senha)
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Usuário é obrigatório")]
        [StringLength(100, ErrorMessage = "Usuário deve ter no máximo 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "Senha deve ter no máximo 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cadastro de usuário
    /// </summary>
    public class CadastrarLoginRequest
    {
        [Required(ErrorMessage = "Usuário é obrigatório")]
        [StringLength(100, ErrorMessage = "Usuário deve ter no máximo 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para login externo (Google, etc.)
    /// </summary>
    public class ExternalLoginRequest
    {
        [Required(ErrorMessage = "IdToken é obrigatório")]
        public string IdToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Provider é obrigatório")]
        public string Provider { get; set; } = "google";

        // Campos opcionais para fallback
        public string? Email { get; set; }
        public string? Usuario { get; set; }
    }

    // =========================================================
    //                    DTOs DE RESET DE SENHA
    // =========================================================

    /// <summary>
    /// DTO para solicitar reset de senha
    /// </summary>
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resetar senha com token
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para confirmar email
    /// </summary>
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; } = string.Empty;
    }

    // =========================================================
    //                    DTOs DE RESPOSTA
    // =========================================================

    /// <summary>
    /// Resposta de login bem-sucedido
    /// </summary>
    public record LoginResponse(
        int LoginId,
        string Usuario,
        int TipoUsuarioId,
        string Token,
        string? RefreshToken = null
    );

    /// <summary>
    /// Resposta de cadastro bem-sucedido
    /// </summary>
    public record CadastrarLoginResponse(
        int LoginId,
        string Usuario,
        string Email,
        int TipoUsuarioId
    );

    /// <summary>
    /// Resposta de perfil do usuário
    /// </summary>
    public record PerfilResponse(
        int LoginId,
        string Usuario,
        string Email,
        int TipoUsuarioId,
        string? AvatarUrl,
        bool EmailConfirmado
    );

    // =========================================================
    //                    DTOs DE PERFIL
    // =========================================================

    /// <summary>
    /// DTO para atualizar perfil
    /// </summary>
    public class AtualizarPerfilRequest
    {
        [StringLength(100, ErrorMessage = "Usuário deve ter no máximo 100 caracteres")]
        public string? Usuario { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string? Senha { get; set; }
    }

    /// <summary>
    /// DTO para atualizar nome do usuário
    /// </summary>
    public class AtualizarNomeRequest
    {
        [Required(ErrorMessage = "Novo nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string NovoNome { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para validar disponibilidade de nome de usuário
    /// </summary>
    public class ValidarUsuarioRequest
    {
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome de usuário deve ter no máximo 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;
    }

    // =========================================================
    //                    DTOs DE RESPOSTA GENÉRICA
    // =========================================================

    /// <summary>
    /// Resposta genérica para operações simples
    /// </summary>
    public record SimpleResponse(
        bool Success,
        string Message,
        object? Data = null
    );

    /// <summary>
    /// Resposta para operações de token
    /// </summary>
    public record TokenResponse(
        string Token,
        string? RefreshToken = null,
        DateTime? ExpiresAt = null
    );
}
