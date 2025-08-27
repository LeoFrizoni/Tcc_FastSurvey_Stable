namespace FASTSURVEY.Dtos.ParticipantesSessao
{
    public class ParticipanteRankingResponse
    {
        public int ParticipanteId { get; set; }
        public string NomeParticipante { get; set; } = string.Empty;
        public int PontuacaoTotal { get; set; }
        public int RespostasCorretas { get; set; }
        public int? TempoMedioResposta { get; set; }
        public int Ranking { get; set; }
    }
}
