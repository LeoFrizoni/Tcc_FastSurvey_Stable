namespace FASTSURVEY.Dtos.Tipos
{
    public class TipoPerguntaDto
    {
        public int TipoPerguntaId { get; set; }
        public string TipoPergunta { get; set; } = string.Empty;
        public bool Desabilitado { get; set; }
    }
}
