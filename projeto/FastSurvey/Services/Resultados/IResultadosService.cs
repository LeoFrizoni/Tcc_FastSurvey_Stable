using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Result;

namespace FASTSURVEY.Services.Resultados
{
    public interface IResultadosService
    {
        Task<ServiceResult<object>> ObterResultadosPesquisaAsync(EstatisticasPesquisaRequest request, CancellationToken ct = default);
        Task<ServiceResult<object>> ObterEstatisticasPerguntaAsync(EstatisticasPerguntaRequest request, CancellationToken ct = default);
        Task<ServiceResult<ExportResult>> ExportarResultadosAsync(EstatisticasPesquisaRequest request, CancellationToken ct = default);
        Task<ServiceResult<object>> ObterGraficosAsync(int pesquisaId, CancellationToken ct = default);
        Task<ServiceResult<DashboardData>> ObterDashboardAsync(int loginId, CancellationToken ct = default);
    }

    public class ExportResult
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class DashboardData
    {
        public int TotalPesquisas { get; set; }
        public int TotalRespostas { get; set; }
        public int PesquisasAtivas { get; set; }
        public int PesquisasInterativas { get; set; }
        public List<PesquisaPopular> PesquisasPopulares { get; set; } = new();
        public List<EstatisticaMensal> EstatisticasMensais { get; set; } = new();
    }

    public class PesquisaPopular
    {
        public int PesquisaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int TotalRespostas { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class EstatisticaMensal
    {
        public string Mes { get; set; } = string.Empty;
        public int PesquisasCriadas { get; set; }
        public int RespostasRecebidas { get; set; }
    }
}
