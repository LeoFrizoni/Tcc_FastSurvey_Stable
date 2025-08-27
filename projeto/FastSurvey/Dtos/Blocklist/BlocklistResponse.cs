namespace FASTSURVEY.Dtos.Blocklist
{
    public class BlocklistResponse
    {
        public int Id { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime? CriadoEm { get; set; }
    }
}
