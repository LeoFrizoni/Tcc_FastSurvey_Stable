namespace FASTSURVEY.Models
{
    public class PesquisaVM
    {
        public string? Titulo { get; set; }
        public string? Autor { get; set; }
        public string? DataCriacao { get; set; }
        public string? Descricao { get; set; }
        public int TipoPesquisaId { get; set; }
        public int CodigoPesquisa { get; set; }
        public int LoginId { get; set; }
        public List<PerguntaDiscursiva>? PerguntasDiscursivas { get; set; }
        public List<PerguntaObjetiva>? PerguntasObjetivas { get; set; }
        public List<PerguntaMultiplaEscolha>? PerguntasMultiplaEscolha { get; set; }
        public string? TemplateJson { get; set; }
    }

}
