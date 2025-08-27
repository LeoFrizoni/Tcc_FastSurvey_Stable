namespace FASTSURVEY.Dtos.ExternalLogins
{
    public class ExternalLoginResponse
    {
        public int ExternalLoginId { get; set; }
        public int LoginId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }
}
