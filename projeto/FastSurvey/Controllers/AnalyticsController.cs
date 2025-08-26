using FASTSURVEY.Dtos.Analytics;
using FASTSURVEY.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class AnalyticsController : BaseController
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            ILogger<AnalyticsController> logger
        )
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        // GET: api/analytics/dashboard
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardAnalyticsResponse>> GetDashboardAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken ct = default
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var analytics = await _analyticsService.GetDashboardAnalyticsAsync(
                    loginId,
                    startDate,
                    endDate,
                    ct
                );
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter analytics do dashboard");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // GET: api/analytics/relatorio/{pesquisaId}
        [HttpGet("relatorio/{pesquisaId:int}")]
        [ProducesResponseType(typeof(RelatorioPesquisaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RelatorioPesquisaResponse>> GetRelatorioPesquisa(
            [FromRoute] int pesquisaId,
            CancellationToken ct = default
        )
        {
            try
            {
                var relatorio = await _analyticsService.GetRelatorioPesquisaAsync(pesquisaId, ct);
                return Ok(relatorio);
            }
            catch (ArgumentException)
            {
                return NotFound(new { message = "Pesquisa não encontrada" });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao gerar relatório da pesquisa {PesquisaId}",
                    pesquisaId
                );
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // POST: api/analytics/relatorio-comparativo
        [HttpPost("relatorio-comparativo")]
        [ProducesResponseType(typeof(List<RelatorioComparativoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RelatorioComparativoResponse>>> GetRelatorioComparativo(
            [FromBody] List<int> pesquisaIds,
            CancellationToken ct = default
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var relatorios = await _analyticsService.GetRelatorioComparativoAsync(
                    loginId,
                    pesquisaIds,
                    ct
                );
                return Ok(relatorios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório comparativo");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // GET: api/analytics/metricas-performance
        [HttpGet("metricas-performance")]
        [ProducesResponseType(typeof(MetricasPerformanceResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<MetricasPerformanceResponse>> GetMetricasPerformance(
            CancellationToken ct = default
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var metricas = await _analyticsService.GetMetricasPerformanceAsync(loginId, ct);
                return Ok(metricas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas de performance");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // POST: api/analytics/agendar-relatorio
        [HttpPost("agendar-relatorio")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AgendarRelatorio(
            [FromBody] AgendarRelatorioRequest request,
            CancellationToken ct = default
        )
        {
            try
            {
                request.LoginId = GetLoginIdFromToken();
                var success = await _analyticsService.AgendarRelatorioAsync(request, ct);

                if (success)
                    return Ok(new { message = "Relatório agendado com sucesso" });
                else
                    return BadRequest(new { message = "Erro ao agendar relatório" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao agendar relatório");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // GET: api/analytics/relatorios-agendados
        [HttpGet("relatorios-agendados")]
        [ProducesResponseType(typeof(List<RelatorioAgendadoResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RelatorioAgendadoResponse>>> GetRelatoriosAgendados(
            CancellationToken ct = default
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var relatorios = await _analyticsService.GetRelatoriosAgendadosAsync(loginId, ct);
                return Ok(relatorios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter relatórios agendados");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // DELETE: api/analytics/relatorio-agendado/{relatorioId}
        [HttpDelete("relatorio-agendado/{relatorioId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelarRelatorioAgendado(
            [FromRoute] int relatorioId,
            CancellationToken ct = default
        )
        {
            try
            {
                var success = await _analyticsService.CancelarRelatorioAgendadoAsync(
                    relatorioId,
                    ct
                );

                if (success)
                    return Ok(new { message = "Relatório cancelado com sucesso" });
                else
                    return NotFound(new { message = "Relatório não encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao cancelar relatório agendado {RelatorioId}",
                    relatorioId
                );
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
