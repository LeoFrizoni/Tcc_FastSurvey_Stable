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
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool TemLimitadorTempo { get; set; }
        public DateTime? DataFechamento { get; set; }
        public bool IsInterativa { get; set; }
        public bool PermiteRespostasAnonimas { get; set; }
        public int? LimiteRespostas { get; set; }
        public bool Ativa { get; set; }
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
