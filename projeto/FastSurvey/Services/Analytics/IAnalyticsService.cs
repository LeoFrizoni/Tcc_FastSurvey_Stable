using FASTSURVEY.Dtos.Analytics;

namespace FASTSURVEY.Services.Analytics
{
    public interface IAnalyticsService
    {
        // Dashboard Analytics
        Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(
            int loginId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken ct = default
        );

        // Relatórios de Pesquisa
        Task<RelatorioPesquisaResponse> GetRelatorioPesquisaAsync(
            int pesquisaId,
            CancellationToken ct = default
        );
        Task<List<RelatorioComparativoResponse>> GetRelatorioComparativoAsync(
            int loginId,
            List<int> pesquisaIds,
            CancellationToken ct = default
        );

        // Métricas de Performance
        Task<MetricasPerformanceResponse> GetMetricasPerformanceAsync(
            int loginId,
            CancellationToken ct = default
        );

        // Agendamento de Relatórios
        Task<bool> AgendarRelatorioAsync(
            AgendarRelatorioRequest request,
            CancellationToken ct = default
        );
        Task<List<RelatorioAgendadoResponse>> GetRelatoriosAgendadosAsync(
            int loginId,
            CancellationToken ct = default
        );
        Task<bool> CancelarRelatorioAgendadoAsync(int relatorioId, CancellationToken ct = default);
    }
}
