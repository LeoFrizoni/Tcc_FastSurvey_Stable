using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultadosController : ControllerBase
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
            CancellationToken ct)
        {
            request.PesquisaId = pesquisaId;
            var result = await _service.ObterResultadosPesquisaAsync(request, ct);
            return Ok(result);
        }

        // GET: api/resultados/pergunta/{perguntaId}
        [HttpGet("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ObterEstatisticasPergunta(
            [FromRoute] int perguntaId,
            [FromQuery] EstatisticasPerguntaRequest request,
            CancellationToken ct)
        {
            request.PerguntaId = perguntaId;
            var result = await _service.ObterEstatisticasPerguntaAsync(request, ct);
            return Ok(result);
        }

        // POST: api/resultados/exportar
        [HttpPost("exportar")]
        public async Task<IActionResult> ExportarResultados(
            [FromBody] EstatisticasPesquisaRequest request,
            CancellationToken ct)
        {
            var result = await _service.ExportarResultadosAsync(request, ct);
            if (!result.Success)
                return BadRequest(result.Errors);
            
            return File(result.Data.Data, result.Data.ContentType, result.Data.FileName);
        }

        // GET: api/resultados/graficos/{pesquisaId}
        [HttpGet("graficos/{pesquisaId:int}")]
        public async Task<IActionResult> ObterGraficos(
            [FromRoute] int pesquisaId,
            CancellationToken ct)
        {
            var result = await _service.ObterGraficosAsync(pesquisaId, ct);
            return Ok(result);
        }
    }
}
