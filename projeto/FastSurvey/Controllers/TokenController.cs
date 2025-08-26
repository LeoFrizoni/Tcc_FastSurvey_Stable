#nullable enable
using FASTSURVEY.Dtos.Tokens;
using FASTSURVEY.Services.Result;
using FASTSURVEY.Services.Tokens;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TokenController : ControllerBase
    {
        private const int TAMANHO_MAX_PAGINA = 200;
        private readonly ITokenService _service;

        public TokenController(ITokenService service) => _service = service;

        // POST api/Token  { validadeMinutos?: number }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar(
            [FromBody] CriarTokenRequest? req,
            CancellationToken ct
        )
        {
            var minutos = req?.ValidadeMinutos ?? 10;
            if (minutos <= 0)
                return Problem(
                    title: "Validade inválida",
                    detail: "Validade deve ser maior que zero.",
                    statusCode: StatusCodes.Status400BadRequest
                );

            var result = await _service.CriarAsync(
                minutos,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // GET api/Token/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.ObterPorIdAsync(
                id,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // GET api/Token?apenasAtivos=true&pagina=1&tamanho=50
        [HttpGet]
        // Se ListarAsync devolver uma página/metadata, troque o tipo abaixo pelo seu wrapper real
        [ProducesResponseType(typeof(IEnumerable<TokenDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] bool apenasAtivos = false,
            [FromQuery] int? pagina = null,
            [FromQuery] int? tamanho = null,
            CancellationToken ct = default
        )
        {
            var pg = Math.Max(1, pagina ?? 1);
            var tz = Math.Clamp(tamanho ?? 50, 1, TAMANHO_MAX_PAGINA);

            var result = await _service.ListarAsync(
                apenasAtivos,
                pg,
                tz,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // GET api/Token/validar?token=abc.xyz
        [HttpGet("validar")]
        [ProducesResponseType(typeof(ValidarTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Validar([FromQuery] string? token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Problem(
                    title: "Token ausente",
                    detail: "Informe o token via querystring (?token=).",
                    statusCode: StatusCodes.Status400BadRequest
                );

            var result = await _service.ValidarAsync(
                token,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // (Opcional) Rota antiga, mantida por compatibilidade
        [HttpGet("Validar/{token}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> ValidarLegacy([FromRoute] string token, CancellationToken ct) =>
            Validar(token, ct);

        // PATCH api/Token/{id}/Prorrogar
        [HttpPatch("{id:int}/Prorrogar")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Prorrogar(
            [FromRoute] int id,
            [FromBody] ProrrogarTokenRequest? req,
            CancellationToken ct
        )
        {
            if (req is null || req.Minutos <= 0)
                return Problem(
                    title: "Dados inválidos",
                    detail: "Informe 'Minutos' maior que zero.",
                    statusCode: StatusCodes.Status400BadRequest
                );

            var result = await _service.ProrrogarAsync(
                id,
                req.Minutos,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // PATCH api/Token/{id}/Revogar
        [HttpPatch("{id:int}/Revogar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revogar([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.RevogarAsync(
                id,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }

        // DELETE api/Token/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remover([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.RemoverAsync(
                id,
                ct == default ? HttpContext.RequestAborted : ct
            );
            return result.ToActionResult(this);
        }
    }
}
