// FASTSURVEY/Api/Controllers/PesquisasController.cs
#nullable enable
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.Pesquisa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/pesquisas
    [Produces("application/json")]
    [Authorize]
    public class PesquisasController : ControllerBase
    {
        private readonly IPesquisaService _service;

        public PesquisasController(IPesquisaService service) => _service = service;

        // GET: api/pesquisas/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PesquisaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "id" })]
        public async Task<ActionResult<PesquisaResponse>> GetById(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var res = await _service.ObterPorIdAsync(id, ct);
            return res is null ? NotFound() : Ok(res);
        }

        // GET: api/pesquisas/slug/{slug}
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(PesquisaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PesquisaResponse>> GetBySlug(
            [FromRoute] string slug,
            CancellationToken ct
        )
        {
            var res = await _service.ObterPorSlugAsync(slug, ct);
            return res is null ? NotFound() : Ok(res);
        }

        // GET: api/pesquisas?LoginId=&PastaId=&TipoPesquisaId=&Busca=&IsInterativa=&Ativa=&TemLimitadorTempo=&DataInicio=&DataFim=&Page=&PageSize=
        [HttpGet]
        [ProducesResponseType(
            typeof(PagedResult<PesquisaListItemResponse>),
            StatusCodes.Status200OK
        )]
        [ResponseCache(
            Duration = 300,
            VaryByQueryKeys = new[]
            {
                "LoginId",
                "PastaId",
                "TipoPesquisaId",
                "Busca",
                "IsInterativa",
                "Ativa",
                "TemLimitadorTempo",
                "DataInicio",
                "DataFim",
                "Page",
                "PageSize",
            }
        )]
        public async Task<ActionResult<PagedResult<PesquisaListItemResponse>>> List(
            [FromQuery] PesquisaFiltroRequest filtro,
            CancellationToken ct
        )
        {
            var page = await _service.ListarAsync(filtro, ct);
            return Ok(page);
        }

        // GET: api/pesquisas/usuario/{loginId}
        [HttpGet("usuario/{loginId:int}")]
        [ProducesResponseType(
            typeof(IEnumerable<PesquisaListItemResponse>),
            StatusCodes.Status200OK
        )]
        [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "loginId" })]
        public async Task<ActionResult<IEnumerable<PesquisaListItemResponse>>> ListarPorUsuario(
            [FromRoute] int loginId,
            CancellationToken ct
        )
        {
            if (loginId <= 0)
                return BadRequest("loginId inválido.");
            var list = await _service.ListarPorLoginAsync(loginId, ct);
            return Ok(list);
        }

        // POST: api/pesquisas
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create(
            [FromBody] CriarPesquisaRequest req,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var id = await _service.CriarAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        // PUT: api/pesquisas/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(
            [FromRoute] int id,
            [FromBody] AtualizarPesquisaRequest req,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var ok = await _service.AtualizarAsync(id, req, ct);
            return ok ? NoContent() : NotFound();
        }

        // PATCH: api/pesquisas/{id}/template
        [HttpPatch("{id:int}/template")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PatchTemplate(
            [FromRoute] int id,
            [FromBody] AtualizarTemplateRequest body,
            CancellationToken ct
        )
        {
            var ok = await _service.AtualizarTemplateAsync(id, body.TemplateJson, ct);
            return ok ? NoContent() : NotFound();
        }

        // PATCH: api/pesquisas/{id}/mover-pasta
        public record MoverPastaRequest(int? PastaId);

        [HttpPatch("{id:int}/mover-pasta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> MoverParaPasta(
            [FromRoute] int id,
            [FromBody] MoverPastaRequest body,
            CancellationToken ct
        )
        {
            var ok = await _service.DefinirPastaAsync(id, body.PastaId, ct);
            return ok ? NoContent() : NotFound();
        }

        // DELETE: api/pesquisas/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _service.ExcluirAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }

        // POST: api/pesquisas/{id}/duplicar
        [HttpPost("{id:int}/duplicar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Duplicar(
            [FromRoute] int id,
            [FromBody] DuplicarPesquisaRequest request,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var novoId = await _service.DuplicarAsync(id, request, ct);
            if (novoId == 0)
                return NotFound();
            return CreatedAtAction(nameof(GetById), new { id = novoId }, new { id = novoId });
        }

        // GET: api/pesquisas/{id}/qr
        // [AllowAnonymous] // habilite se o front público precisar
        [HttpGet("{id:int}/qr")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ObterQRCode([FromRoute] int id, CancellationToken ct)
        {
            var qrCode = await _service.GerarQRCodeAsync(id, ct);
            return qrCode is null ? NotFound() : Ok(qrCode);
        }

        // GET: api/pesquisas/{id}/export/pdf
        [HttpGet("{id:int}/export/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExportarPDF([FromRoute] int id, CancellationToken ct)
        {
            var pdfBytes = await _service.ExportarPDFAsync(id, ct);
            return pdfBytes is null
                ? NotFound()
                : File(pdfBytes, "application/pdf", $"pesquisa_{id}.pdf");
        }

        // POST: api/pesquisas/{id}/validar
        // [AllowAnonymous] // habilite se o front público precisar
        [HttpPost("{id:int}/validar")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ValidarAcesso(
            [FromRoute] int id,
            [FromBody] ValidarPesquisaRequest request,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var resultado = await _service.ValidarAcessoAsync(id, request, ct);
            return resultado is null ? NotFound() : Ok(resultado);
        }

        // GET: api/pesquisas/{id}/estatisticas
        [HttpGet("{id:int}/estatisticas")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ObterEstatisticas(
            [FromRoute] int id,
            [FromQuery] EstatisticasPesquisaRequest request,
            CancellationToken ct
        )
        {
            var estatisticas = await _service.ObterEstatisticasAsync(id, request, ct);
            return estatisticas is null ? NotFound() : Ok(estatisticas);
        }

        // POST: api/pesquisas/{id}/reprocessar-perguntas
        // Endpoint temporário para reprocessar pesquisas existentes
        [HttpPost("{id:int}/reprocessar-perguntas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReprocessarPerguntas(
            [FromRoute] int id,
            CancellationToken ct
        )
        {
            var resultado = await _service.ReprocessarPerguntasAsync(id, ct);
            return resultado ? Ok(new { message = "Perguntas reprocessadas com sucesso" }) : NotFound();
        }
    }
}
