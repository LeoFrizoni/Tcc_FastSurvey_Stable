using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.FeatureFlags
{
    public class FeatureFlagRequest
    {
        [Required(ErrorMessage = "O ID do login é obrigatório")]
        public int LoginId { get; set; }

        [Required(ErrorMessage = "O nome da flag é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome da flag deve ter no máximo 100 caracteres")]
        public string Flag { get; set; } = string.Empty;

        [Required(ErrorMessage = "O status da flag é obrigatório")]
        public bool Ativa { get; set; }
    }
}
