using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Result;
using FASTSURVEY.Services.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ResultadosController : BaseController
    {
        private readonly IResultadosService _service;

        public ResultadosController(IResultadosService service)
        {
            _service = service;
        }

        // GET: api/resultados/pesquisa/{pesquisaId}
        [HttpGet("pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> ObterResultadosPesquisa(
            [FromRoute] int pesquisaId,
            [FromQuery] EstatisticasPesquisaRequest request,
            CancellationToken ct
        )
        {
            request.PesquisaId = pesquisaId;
            var result = await _service.ObterResultadosPesquisaAsync(request, ct);
            return result.ToActionResult(this);
        }

        // GET: api/resultados/pergunta/{perguntaId}
        [HttpGet("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ObterEstatisticasPergunta(
            [FromRoute] int perguntaId,
            [FromQuery] EstatisticasPerguntaRequest request,
            CancellationToken ct
        )
        {
            request.PerguntaId = perguntaId;
            var result = await _service.ObterEstatisticasPerguntaAsync(request, ct);
            return result.ToActionResult(this);
        }

        // POST: api/resultados/exportar
        [HttpPost("exportar")]
        public async Task<IActionResult> ExportarResultados(
            [FromBody] EstatisticasPesquisaRequest request,
            CancellationToken ct
        )
        {
            var result = await _service.ExportarResultadosAsync(request, ct);
            if (!result.Success)
                return result.ToActionResult(this);

            var file = result.Data!;
            return File(file.Data, file.ContentType, file.FileName);
        }

        // GET: api/resultados/exportar-pdf/{pesquisaId}
        [HttpGet("exportar-pdf/{pesquisaId:int}")]
        public async Task<IActionResult> ExportarResultadosPDF(
            [FromRoute] int pesquisaId,
            CancellationToken ct
        )
        {
            var request = new EstatisticasPesquisaRequest
            {
                PesquisaId = pesquisaId,
                Formato = "pdf",
            };
            var result = await _service.ExportarResultadosAsync(request, ct);
            if (!result.Success)
                return result.ToActionResult(this);

            // Usa o nome de arquivo sugerido pelo service quando existir
            var file = result.Data!;
            var fileName = string.IsNullOrWhiteSpace(file.FileName)
                ? $"resultados-pesquisa-{pesquisaId}.pdf"
                : file.FileName;

            return File(file.Data, "application/pdf", fileName);
        }

        // GET: api/resultados/exportar-excel/{pesquisaId}
        [HttpGet("exportar-excel/{pesquisaId:int}")]
        public async Task<IActionResult> ExportarResultadosExcel(
            [FromRoute] int pesquisaId,
            CancellationToken ct
        )
        {
            var request = new EstatisticasPesquisaRequest
            {
                PesquisaId = pesquisaId,
                Formato = "excel",
            };
            var result = await _service.ExportarResultadosAsync(request, ct);
            if (!result.Success)
                return result.ToActionResult(this);

            var file = result.Data!;
            var fileName = string.IsNullOrWhiteSpace(file.FileName)
                ? $"resultados-pesquisa-{pesquisaId}.xlsx"
                : file.FileName;

            return File(
                file.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // GET: api/resultados/graficos/{pesquisaId}
        [HttpGet("graficos/{pesquisaId:int}")]
        [AllowAnonymous] // se quiser liberar gráficos para dashboards públicos
        public async Task<IActionResult> ObterGraficos(
            [FromRoute] int pesquisaId,
            CancellationToken ct
        )
        {
            var result = await _service.ObterGraficosAsync(pesquisaId, ct);
            return result.ToActionResult(this);
        }

        // (Opcional) GET: api/resultados/relatorio-completo/{pesquisaId}
        [HttpGet("relatorio-completo/{pesquisaId:int}")]
        public async Task<IActionResult> ObterRelatorioCompleto(
            [FromRoute] int pesquisaId,
            CancellationToken ct
        )
        {
            var result = await _service.ObterRelatorioCompletoAsync(pesquisaId, ct);
            return result.ToActionResult(this);
        }
    }
}
