using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.ExternalLogins
{
    public class ExternalLoginRequest
    {
        [Required(ErrorMessage = "O ID do login é obrigatório")]
        public int LoginId { get; set; }

        [Required(ErrorMessage = "O provedor é obrigatório")]
        [StringLength(50, ErrorMessage = "O provedor deve ter no máximo 50 caracteres")]
        public string Provider { get; set; } = string.Empty;

        [Required(ErrorMessage = "O ID do usuário no provedor é obrigatório")]
        [StringLength(255, ErrorMessage = "O ID do usuário deve ter no máximo 255 caracteres")]
        public string ProviderUserId { get; set; } = string.Empty;
    }
}
