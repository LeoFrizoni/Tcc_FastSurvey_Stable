#nullable enable
using FASTSURVEY.Dtos.Tipos;
using FASTSURVEY.Services.Tipos;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPesquisaController : ControllerBase
    {
        private readonly ITipoPesquisaService _service;
        public TipoPesquisaController(ITipoPesquisaService service) => _service = service;

        /// <summary>
        /// Usado pelo front (ModalCriarPesquisa e CriarPesquisa):
        /// GET api/TipoPesquisa/ListarTipoPesquisa
        /// </summary>
        [HttpGet("ListarTipoPesquisa")]
        public async Task<ActionResult<IEnumerable<TipoPesquisaDto>>> ListarTipoPesquisa(CancellationToken ct)
        {
            var lista = await _service.ListarAsync(incluirDesabilitados: false, ct);

            var dtos = lista.Select(x => new TipoPesquisaDto
            {
                tipopesquisaid = x.Tipopesquisaid,
                tipopesquisa = x.Tipopesquisa1 ?? string.Empty,
                desabilitado = x.Desabilitado
            });

            return Ok(dtos);
        }

        // Endpoint auxiliar: GET api/TipoPesquisa
        [HttpGet]
        public Task<ActionResult<IEnumerable<TipoPesquisaDto>>> Get(CancellationToken ct)
            => ListarTipoPesquisa(ct);
    }
}
