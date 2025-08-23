namespace FASTSURVEY.Dtos.Anexos
{
    public class AnexoResponse
    {
        public int Id { get; set; }
        public int? PesquisaId { get; set; }
        public int? PerguntaId { get; set; }

        public string Nome { get; set; } = "";
        public string Extensao { get; set; } = "";
        public string? NomeOriginal { get; set; }
        public string? ContentType { get; set; }
        public long? TamanhoBytes { get; set; }
        public string? Base64Data { get; set; }
        public string? Url { get; set; } = "";
    }
}
