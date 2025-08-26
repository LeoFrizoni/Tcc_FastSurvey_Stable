namespace FASTSURVEY.Dtos.Analytics
{
    public class DashboardAnalyticsResponse
    {
        public int TotalPesquisas { get; set; }
        public int TotalRespostas { get; set; }
        public int PesquisasAtivas { get; set; }
        public double TaxaRespostaMedia { get; set; }
        public List<GraficoRespostasPorDia> RespostasPorDia { get; set; } = new();
        public List<GraficoTipoPesquisa> PesquisasPorTipo { get; set; } = new();
        public List<GraficoPerformance> PerformanceMensal { get; set; } = new();
        public List<PesquisaRecente> PesquisasRecentes { get; set; } = new();
    }

    public class GraficoRespostasPorDia
    {
        public DateTime Data { get; set; }
        public int TotalRespostas { get; set; }
        public int PesquisasAtivas { get; set; }
    }

    public class GraficoTipoPesquisa
    {
        public string TipoPesquisa { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public double Percentual { get; set; }
    }

    public class GraficoPerformance
    {
        public string Mes { get; set; } = string.Empty;
        public int PesquisasCriadas { get; set; }
        public int RespostasRecebidas { get; set; }
        public double TaxaEngajamento { get; set; }
    }

    public class PesquisaRecente
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int TotalRespostas { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RelatorioPesquisaResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public int TotalRespostas { get; set; }
        public double TaxaResposta { get; set; }
        public List<EstatisticaPergunta> EstatisticasPerguntas { get; set; } = new();
        public List<RespostaRecente> RespostasRecentes { get; set; } = new();
        public Dictionary<string, object> MetricasGerais { get; set; } = new();
    }

    public class EstatisticaPergunta
    {
        public int PerguntaId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public string TipoPergunta { get; set; } = string.Empty;
        public int TotalRespostas { get; set; }
        public List<OpcaoEstatistica> Opcoes { get; set; } = new();
        public List<string> RespostasDiscursivas { get; set; } = new();
    }

    public class OpcaoEstatistica
    {
        public string Texto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public double Percentual { get; set; }
    }

    public class RespostaRecente
    {
        public DateTime DataResposta { get; set; }
        public string Resposta { get; set; } = string.Empty;
        public string Pergunta { get; set; } = string.Empty;
    }

    public class RelatorioComparativoResponse
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int TotalRespostas { get; set; }
        public double TaxaResposta { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<ComparacaoPergunta> Comparacoes { get; set; } = new();
    }

    public class ComparacaoPergunta
    {
        public string Pergunta { get; set; } = string.Empty;
        public Dictionary<string, double> ResultadosPorPesquisa { get; set; } = new();
    }

    public class MetricasPerformanceResponse
    {
        public double TempoMedioResposta { get; set; }
        public double TaxaConclusao { get; set; }
        public int PesquisasMaisPopulares { get; set; }
        public List<MetricaTempoReal> MetricasTempoReal { get; set; } = new();
    }

    public class MetricaTempoReal
    {
        public DateTime Timestamp { get; set; }
        public int RespostasAtivas { get; set; }
        public int UsuariosConectados { get; set; }
    }

    public class AgendarRelatorioRequest
    {
        public int LoginId { get; set; }
        public int? PesquisaId { get; set; }
        public string TipoRelatorio { get; set; } = string.Empty;
        public string Frequencia { get; set; } = string.Empty; // diario, semanal, mensal
        public string EmailDestino { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Formato { get; set; } = "PDF"; // PDF, Excel
    }

    public class RelatorioAgendadoResponse
    {
        public int RelatorioId { get; set; }
        public string TipoRelatorio { get; set; } = string.Empty;
        public string Frequencia { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ProximaExecucao { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
