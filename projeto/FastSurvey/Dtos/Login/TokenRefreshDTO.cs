using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Auth
{
    public class TokenRefreshDTO
    {
        [Required] public string RefreshToken { get; set; } = "";
    }
}
