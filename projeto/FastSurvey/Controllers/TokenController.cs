#nullable enable
using FASTSURVEY.Dtos.Tokens;
using FASTSURVEY.Services.Result;
using FASTSURVEY.Services.Tokens;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _service;
        public TokenController(ITokenService service) => _service = service;

        // POST api/Token  { validadeMinutos?: number }
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarTokenRequest req, CancellationToken ct)
        {
            var result = await _service.CriarAsync(req?.validadeMinutos ?? 10, ct);
            return result.ToActionResult(this);
        }

        // GET api/Token/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.ObterPorIdAsync(id, ct);
            return result.ToActionResult(this);
        }

        // GET api/Token?apenasAtivos=true&pagina=1&tamanho=50
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] bool apenasAtivos = false, [FromQuery] int? pagina = null, [FromQuery] int? tamanho = null, CancellationToken ct = default)
        {
            var result = await _service.ListarAsync(apenasAtivos, pagina, tamanho, ct);
            return result.ToActionResult(this);
        }

        // GET api/Token/Validar/{token}
        [HttpGet("Validar/{token}")]
        public async Task<IActionResult> Validar([FromRoute] string token, CancellationToken ct)
        {
            var result = await _service.ValidarAsync(token, ct);
            return result.ToActionResult(this);
        }

        // PATCH api/Token/{id}/Prorrogar
        [HttpPatch("{id:int}/Prorrogar")]
        public async Task<IActionResult> Prorrogar([FromRoute] int id, [FromBody] ProrrogarTokenRequest req, CancellationToken ct)
        {
            var result = await _service.ProrrogarAsync(id, req.minutos, ct);
            return result.ToActionResult(this);
        }

        // PATCH api/Token/{id}/Revogar
        [HttpPatch("{id:int}/Revogar")]
        public async Task<IActionResult> Revogar([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.RevogarAsync(id, ct);
            return result.ToActionResult(this);
        }

        // DELETE api/Token/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Remover([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.RemoverAsync(id, ct);
            return result.ToActionResult(this);
        }
    }
}
