#nullable enable
namespace FASTSURVEY.Dtos.Respostas
{
    public class ValidacaoRespostaResponse
    {
        public bool Valida { get; set; }
        public List<string> Erros { get; set; } = new();
        public List<string> Avisos { get; set; } = new();
        public object? Dados { get; set; }
    }

    public class StatusPesquisaResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public bool Ativa { get; set; }
        public bool Expirada { get; set; }
        public DateTime? DataExpiracao { get; set; }
        public int TotalRespostas { get; set; }
        public int? LimiteRespostas { get; set; }
        public bool LimiteAtingido { get; set; }
        public bool RequerIdentificacao { get; set; }
        public string? Instrucoes { get; set; }
        public bool PermiteRespostasAnonimas { get; set; }
    }

    public class AnalyticsResponse
    {
        public int PesquisaId { get; set; }
        public int TotalRespostas { get; set; }
        public double TaxaConclusao { get; set; } // %
        public TimeSpan TempoMedioResposta { get; set; }
        public Dictionary<string, int> RespostasPorOrigem { get; set; } = new(); // QR, Link, Interativo
        public Dictionary<string, int> RespostasPorPeriodo { get; set; } = new(); // yyyy-MM-dd -> count
        public List<EstatisticaPergunta> EstatisticasPerguntas { get; set; } = new();
    }

    public class EstatisticaPergunta
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int TipoPerguntaId { get; set; }
        public int TotalRespostas { get; set; }
        public double TaxaResposta { get; set; } // %
        public object? DadosGrafico { get; set; }
    }
}
