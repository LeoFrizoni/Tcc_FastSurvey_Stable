// FASTSURVEY/Api/Controllers/PesquisasController.cs
#nullable enable
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Pesquisa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/pesquisas
    [Produces("application/json")]
    [Authorize]
    public class PesquisasController : ControllerBase
    {
        private readonly IPesquisaService _service;

        public PesquisasController(IPesquisaService service)
        {
            _service = service;
        }

        // GET: api/pesquisas/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PesquisaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PesquisaResponse>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var res = await _service.ObterPorIdAsync(id, ct);
            if (res is null) return NotFound();
            return Ok(res);
        }

        // GET: api/pesquisas?LoginId=&PastaId=&TipoPesquisaId=&Busca=&Page=&PageSize=
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<PesquisaListItemResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<PesquisaListItemResponse>>> List(
            [FromQuery] PesquisaFiltroRequest filtro,
            CancellationToken ct)
        {
            var page = await _service.ListarAsync(filtro, ct);
            return Ok(page);
        }

        // ===== Endpoint específico usado pelo Home.jsx =====
        // GET: api/pesquisas/usuario/{loginId}
        [HttpGet("usuario/{loginId:int}")]
        [ProducesResponseType(typeof(IEnumerable<PesquisaListItemResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PesquisaListItemResponse>>> ListarPorUsuario(
            [FromRoute] int loginId, CancellationToken ct)
        {
            if (loginId <= 0) return BadRequest("loginId inválido.");
            var list = await _service.ListarPorLoginAsync(loginId, ct);
            return Ok(list);
        }

        // POST: api/pesquisas
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] CriarPesquisaRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = await _service.CriarAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT: api/pesquisas/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] AtualizarPesquisaRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var ok = await _service.AtualizarAsync(id, req, ct);
            if (!ok) return NotFound();

            return NoContent();
        }

        // PATCH: api/pesquisas/{id}/template
        [HttpPatch("{id:int}/template")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PatchTemplate([FromRoute] int id, [FromBody] AtualizarTemplateRequest body, CancellationToken ct)
        {
            var ok = await _service.AtualizarTemplateAsync(id, body.TemplateJson, ct);
            if (!ok) return NotFound();

            return NoContent();
        }

        // ===== Endpoint específico usado pelo Home.jsx =====
        // PATCH: api/pesquisas/{id}/mover-pasta  body: { pastaId: 123 | null }
        public record MoverPastaRequest(int? PastaId);

        [HttpPatch("{id:int}/mover-pasta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MoverParaPasta([FromRoute] int id, [FromBody] MoverPastaRequest body, CancellationToken ct)
        {
            var ok = await _service.DefinirPastaAsync(id, body.PastaId, ct);
            if (!ok) return NotFound();

            return NoContent();
        }

        // DELETE: api/pesquisas/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _service.ExcluirAsync(id, ct);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
