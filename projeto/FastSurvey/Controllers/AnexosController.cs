using FASTSURVEY.Dtos.Anexos;
using FASTSURVEY.Services.Anexo;
using FASTSURVEY.Services.Result; // ToActionResult extension
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // mantém consistente com o resto da API
    public class AnexosController : ControllerBase
    {
        private readonly IAnexoService _service;

        public AnexosController(IAnexoService service) => _service = service;

        /// <summary>Upload de anexo (PNG, JPG, PDF). Enviar via multipart/form-data com campo "anexo".</summary>
        [AllowAnonymous]
        [HttpPost(Name = "UploadAnexo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20MB no transporte; DTO valida 15MB
        [ProducesResponseType(typeof(AnexoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(
            [FromBody] AnexoUploadRequest req,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var result = await _service.UploadAsync(req, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Obtém metadados do anexo pelo id.</summary>
        [HttpGet("{id:int}", Name = "GetAnexoById")]
        [ProducesResponseType(typeof(AnexoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Lista anexos de uma pesquisa.</summary>
        [HttpGet("por-pesquisa/{pesquisaId:int}", Name = "ListAnexosPorPesquisa")]
        [ProducesResponseType(typeof(List<AnexoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListByPesquisa(
            [FromRoute] int pesquisaId,
            CancellationToken ct = default
        )
        {
            var result = await _service.ListByPesquisaAsync(pesquisaId, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Lista anexos de uma pergunta.</summary>
        [HttpGet("por-pergunta/{perguntaId:int}", Name = "ListAnexosPorPergunta")]
        [ProducesResponseType(typeof(List<AnexoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListByPergunta(
            [FromRoute] int perguntaId,
            CancellationToken ct = default
        )
        {
            var result = await _service.ListByPerguntaAsync(perguntaId, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Exclui um anexo.</summary>
        [HttpDelete("{id:int}", Name = "DeleteAnexo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
        {
            var result = await _service.DeleteAsync(id, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Baixa o arquivo do anexo.</summary>
        [HttpGet("{id:int}/download", Name = "DownloadAnexo")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(
            [FromRoute] int id,
            CancellationToken ct = default
        )
        {
            var result = await _service.DownloadAsync(id, ct);
            if (!result.Success)
                return result.ToActionResult(this);

            var (bytes, contentType, fileName) = result.Data;
            return File(bytes, contentType, fileName);
        }
    }
}
