namespace FASTSURVEY.Dtos.Tipos
{
    public class TipoPesquisaDto
    {
        public int TipoPesquisaId { get; set; }
        public string TipoPesquisa { get; set; } = string.Empty;
        public bool Desabilitado { get; set; }
    }
}
