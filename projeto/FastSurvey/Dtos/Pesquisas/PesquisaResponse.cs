namespace FASTSURVEY.Dtos.Pesquisas
{  // ---------- RESPONSES ----------
    public class PesquisaListItemResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int LoginId { get; set; }
        public int TipoPesquisaId { get; set; }
        public int? PastaId { get; set; }
        public string? QrCodeUrl { get; set; }
        public DateTime Datacriacao { get; set; }
        public DateTime? Dataatualizacao { get; set; }
    }

    public class PesquisaResponse : PesquisaListItemResponse
    {
        public string TemplateJson { get; set; } = "[]";
    }

    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
