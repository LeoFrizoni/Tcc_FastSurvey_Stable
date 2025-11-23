namespace FASTSURVEY.Dtos.Pesquisas
{ // ---------- RESPONSES ----------
    public class PesquisaListItemResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int LoginId { get; set; }
        public int TipoPesquisaId { get; set; }
        public int? PastaId { get; set; }
        public string? QRCodeUrl { get; set; }
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
        public int? PesoMaximoProva { get; set; }

        // Exponha estes se existirem na Model:
        public string? Slug { get; set; }
        public bool? RequerIdentificacao { get; set; }
        public string? Instrucoes { get; set; }
        public bool? MostrarProgresso { get; set; }
        public bool? PermitirEdicao { get; set; }
        public int? TempoLimitePorPergunta { get; set; }
    }

    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }

    // (placeholders usados no Service)
    public class EstatisticasPesquisaRequest { }

    public class StatusPesquisaResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public bool Expirada { get; set; }
        public DateTime? DataExpiracao { get; set; }
        public int? TotalRespostas { get; set; }
        public int? LimiteRespostas { get; set; }
        public bool LimiteAtingido { get; set; }
        public bool RequerIdentificacao { get; set; }
        public string? Instrucoes { get; set; }
        public bool PermiteRespostasAnonimas { get; set; }
    }
}
