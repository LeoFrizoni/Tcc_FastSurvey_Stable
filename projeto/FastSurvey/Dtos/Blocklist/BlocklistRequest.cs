using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Blocklist
{
    public class BlocklistRequest
    {
        [Required(ErrorMessage = "O endereço IP é obrigatório")]
        [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", 
            ErrorMessage = "Formato de IP inválido")]
        public string IPAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "O motivo do bloqueio é obrigatório")]
        [StringLength(500, ErrorMessage = "O motivo deve ter no máximo 500 caracteres")]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "O User Agent deve ter no máximo 500 caracteres")]
        public string? UserAgent { get; set; }
    }
}
