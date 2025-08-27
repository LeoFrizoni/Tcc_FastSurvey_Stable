using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.FeatureFlags
{
    public class FeatureFlagStatusRequest
    {
        [Required(ErrorMessage = "O ID do login é obrigatório")]
        public int LoginId { get; set; }

        [Required(ErrorMessage = "O nome da flag é obrigatório")]
        public string Flag { get; set; } = string.Empty;
    }
}
