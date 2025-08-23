using FASTSURVEY.Dtos.Anexos;
using FASTSURVEY.Services;          // ToActionResult
using FASTSURVEY.Services.Anexo;
using FASTSURVEY.Services.Result;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnexosController : ControllerBase
    {
        private readonly IAnexoService _service;

        public AnexosController(IAnexoService service) => _service = service;

        [HttpPost]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> Upload([FromForm] AnexoUploadRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = await _service.UploadAsync(req, ct);
            return result.ToActionResult(this);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result.ToActionResult(this);
        }

        [HttpGet("por-pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> ListByPesquisa([FromRoute] int pesquisaId, CancellationToken ct)
        {
            var result = await _service.ListByPesquisaAsync(pesquisaId, ct);
            return result.ToActionResult(this);
        }

        [HttpGet("por-pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ListByPergunta([FromRoute] int perguntaId, CancellationToken ct)
        {
            var result = await _service.ListByPerguntaAsync(perguntaId, ct);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.DeleteAsync(id, ct);
            return result.ToActionResult(this);
        }   

        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> Download([FromRoute] int id, CancellationToken ct)
        {
            var result = await _service.DownloadAsync(id, ct);
            if (!result.Success) return result.ToActionResult(this);

            var (bytes, contentType, fileName) = result.Data;
            return File(bytes, contentType, fileName);
        }
    }
}
