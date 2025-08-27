namespace FASTSURVEY.Dtos.ParticipantesSessao
{
    public class ParticipanteResponse
    {
        public int ParticipanteId { get; set; }
        public string SessaoId { get; set; } = string.Empty;
        public string NomeParticipante { get; set; } = string.Empty;
        public DateTime EntrouEm { get; set; }
        public DateTime? SaiuEm { get; set; }
        public int PontuacaoTotal { get; set; }
        public int RespostasCorretas { get; set; }
        public int? TempoMedioResposta { get; set; }
        public int? Ranking { get; set; }
    }
}
