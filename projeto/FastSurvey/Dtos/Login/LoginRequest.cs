using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Login
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Usuário é obrigatório")]
        [StringLength(100, ErrorMessage = "Usuário deve ter no máximo 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "Senha deve ter no máximo 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
    }

    // cadastro via multipart; se preferir JSON, criamos um record e um [FromBody]
    public class CadastrarLoginRequest
    {
        [FromForm(Name = "usuario")] 
        [Required(ErrorMessage = "Usuário é obrigatório")]
        [StringLength(100, ErrorMessage = "Usuário deve ter no máximo 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;
        
        [FromForm(Name = "senha")] 
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "Senha deve ter no máximo 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
        
        [FromForm(Name = "email")] 
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; } = string.Empty;
    }

    public class ExternalLoginRequest
    {
        [Required(ErrorMessage = "Provider é obrigatório")]
        public string Provider { get; set; } = "google";
        
        [Required(ErrorMessage = "IdToken é obrigatório")]
        public string IdToken { get; set; } = string.Empty;

        // Campos auxiliares (fallback, caso não valide o id_token no backend)
        public string? Email { get; set; }
        public string? Usuario { get; set; }
    }

    // Record posicional para resposta do login
    public record LoginResponse(
        int Id,
        string Usuario,
        int TipoUsuarioId,
        string Token
    );

    // Record posicional para resposta do cadastro
    public record CadastrarLoginResponse(
        int Id,
        string Usuario,
        string Email,
        int TipoUsuarioId
    );

    // Record para resposta de perfil
    public record PerfilResponse(
        int Id,
        string Usuario,
        string Email,
        int TipoUsuarioId,
        string? AvatarUrl
    );

    // Classe para resposta de perfil com avatar
    public class PerfilAvatarResponse
    {
        public string Usuario { get; set; } = "";
        public int TipoUsuarioId { get; set; }
        public string Status { get; set; } = "Ativo";
        public string? Url { get; set; }
        public int? Versao { get; set; }
    }

    public class PerfilNomeRequest
    {
        public string Usuario { get; set; } = "";
    }
}
