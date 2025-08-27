using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Login
{
    /// <summary>
    /// DTO para teste de email
    /// </summary>
    public class TestEmailRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }
}
