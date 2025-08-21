using FASTSURVEY.Dtos.Pastas;
using FASTSURVEY.Services.Pasta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PastasController : ControllerBase
    {
        private readonly IPastaService _service;

        public PastasController(IPastaService service) => _service = service;

        // GET /api/pastas?loginid=123
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<PastaResponse>>> Listar([FromQuery] int loginid, CancellationToken ct)
        {
            if (loginid <= 0) return BadRequest("loginid é obrigatório.");
            var list = await _service.ListarAsync(loginid, ct);
            return Ok(list);
        }

        // POST /api/pastas
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PastaResponse>> Criar([FromBody] CriarPastaRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var created = await _service.CriarAsync(req, ct);
                return CreatedAtAction(nameof(Listar), new { loginid = req.LoginId }, created);
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        // PUT /api/pastas/{id}/nome?loginid=123
        [HttpPut("{id:int}/nome")]
        [Authorize]
        public async Task<IActionResult> Renomear([FromRoute] int id, [FromQuery] int loginid,
                                                 [FromBody] RenomearPastaRequest req, CancellationToken ct)
        {
            if (loginid <= 0) return BadRequest("loginid é obrigatório.");
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ok = await _service.RenomearAsync(id, loginid, req.NovoNome, ct);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /api/pastas/{id}?loginid=123
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Excluir([FromRoute] int id, [FromQuery] int loginid, CancellationToken ct)
        {
            if (loginid <= 0) return BadRequest("loginid é obrigatório.");
            var ok = await _service.ExcluirAsync(id, loginid, ct);
            return ok ? NoContent() : NotFound();
        }
    }
}
