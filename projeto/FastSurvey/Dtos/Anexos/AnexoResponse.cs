namespace FASTSURVEY.Dtos.Anexos
{
    public class AnexoResponse
    {
        public int Id { get; set; }
        public int? PesquisaId { get; set; }
        public int? PerguntaId { get; set; }

        public string NomeArquivo { get; set; } = "";
        public string Url { get; set; } = "";
        public string? ContentType { get; set; }
        public long? TamanhoBytes { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
