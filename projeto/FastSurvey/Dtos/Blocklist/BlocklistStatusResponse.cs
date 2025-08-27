namespace FASTSURVEY.Dtos.Blocklist
{
    public class BlocklistStatusResponse
    {
        public string IPAddress { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
        public string? Motivo { get; set; }
        public DateTime? BloqueadoEm { get; set; }
    }
}
