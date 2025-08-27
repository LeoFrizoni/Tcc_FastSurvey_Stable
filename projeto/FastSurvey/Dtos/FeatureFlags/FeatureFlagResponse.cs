namespace FASTSURVEY.Dtos.FeatureFlags
{
    public class FeatureFlagResponse
    {
        public int LoginId { get; set; }
        public string Flag { get; set; } = string.Empty;
        public bool Ativa { get; set; }
    }
}
