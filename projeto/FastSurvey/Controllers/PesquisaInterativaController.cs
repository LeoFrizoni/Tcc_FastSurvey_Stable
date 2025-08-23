using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.PesquisaInterativa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PesquisaInterativaController : ControllerBase
    {
        private readonly IPesquisaInterativaService _service;

        public PesquisaInterativaController(IPesquisaInterativaService service)
        {
            _service = service;
        }

        // POST: api/pesquisainterativa/iniciar
        [HttpPost("iniciar")]
        [Authorize]
        public async Task<IActionResult> IniciarSessao(
            [FromBody] PesquisaInterativaRequest request,
            CancellationToken ct)
        {
            var result = await _service.IniciarSessaoAsync(request, ct);
            return Ok(result);
        }

        // POST: api/pesquisainterativa/entrar
        [HttpPost("entrar")]
        [AllowAnonymous]
        public async Task<IActionResult> EntrarSessao(
            [FromBody] EntrarSessaoRequest request,
            CancellationToken ct)
        {
            var result = await _service.EntrarSessaoAsync(request, ct);
            return Ok(result);
        }

        // GET: api/pesquisainterativa/sessao/{codigo}
        [HttpGet("sessao/{codigo}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterSessao(
            [FromRoute] string codigo,
            CancellationToken ct)
        {
            var result = await _service.ObterSessaoAsync(codigo, ct);
            return Ok(result);
        }

        // POST: api/pesquisainterativa/responder
        [HttpPost("responder")]
        [AllowAnonymous]
        public async Task<IActionResult> ResponderPergunta(
            [FromBody] RespostaInterativaRequest request,
            CancellationToken ct)
        {
            var result = await _service.ResponderPerguntaAsync(request, ct);
            return Ok(result);
        }

        // GET: api/pesquisainterativa/resultados/{sessaoId}
        [HttpGet("resultados/{sessaoId}")]
        [Authorize]
        public async Task<IActionResult> ObterResultadosTempoReal(
            [FromRoute] string sessaoId,
            CancellationToken ct)
        {
            var result = await _service.ObterResultadosTempoRealAsync(sessaoId, ct);
            return Ok(result);
        }

        // POST: api/pesquisainterativa/finalizar
        [HttpPost("finalizar")]
        [Authorize]
        public async Task<IActionResult> FinalizarSessao(
            [FromBody] FinalizarSessaoRequest request,
            CancellationToken ct)
        {
            var result = await _service.FinalizarSessaoAsync(request, ct);
            return Ok(result);
        }
    }

    // DTOs específicos para pesquisa interativa
    public class EntrarSessaoRequest
    {
        public string CodigoAcesso { get; set; } = string.Empty;
        public string NomeParticipante { get; set; } = string.Empty;
    }

    public class RespostaInterativaRequest
    {
        public string SessaoId { get; set; } = string.Empty;
        public int PerguntaId { get; set; }
        public List<int> OpcoesSelecionadas { get; set; } = new();
        public string? TextoResposta { get; set; }
    }

    public class FinalizarSessaoRequest
    {
        public string SessaoId { get; set; } = string.Empty;
        public bool SalvarResultados { get; set; } = true;
    }
}
