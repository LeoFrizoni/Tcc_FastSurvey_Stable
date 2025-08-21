using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Respostas
{
    public class CriarRespostaDiscursivaRequest
    {
        [Required] public int PerguntaId { get; set; }
        [Required, MinLength(1)] public string Texto { get; set; } = string.Empty;

        // metadados opcionais (não persistidos aqui)
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
    }

    public class CriarRespostaOpcoesRequest
    {
        [Required] public int PerguntaId { get; set; }
        [Required] public List<int> OpcoesSelecionadas { get; set; } = new();

        // metadados opcionais (não persistidos aqui)
        public int? LoginId { get; set; }
        public int? PesquisaId { get; set; }
    }

    public class RespostaDto
    {
        public int RespostaId { get; set; }
        public int PerguntaId { get; set; }
        public string? Texto { get; set; }
        public DateTime DataResposta { get; set; }
        public List<int> Opcoes { get; set; } = new();
    }
}
