using FASTSURVEY.Dtos.Tipos;
using FASTSURVEY.Services.Tipos;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPerguntaController : ControllerBase
    {
        private readonly ITipoPerguntaService _service;
        public TipoPerguntaController(ITipoPerguntaService service) => _service = service;

        /// <summary>Lista tipos de pergunta ativos.</summary>
        [HttpGet("Listar")]
        [ProducesResponseType(typeof(IEnumerable<TipoPerguntaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TipoPerguntaDto>>> Listar(CancellationToken ct)
        {
            var lista = await _service.ListarAsync(incluirDesabilitados: false, ct);
            var dtos = lista.Select(x => new TipoPerguntaDto
            {
                tipoperguntaid = x.Tipoperguntaid,
                tipopergunta = x.Tipopergunta1 ?? string.Empty,
                desabilitado = x.Desabilitado
            });
            return Ok(dtos);
        }

        /// <summary>Lista todos (inclui desabilitados) — via querystring ?incluirDesabilitados=true.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TipoPerguntaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TipoPerguntaDto>>> Get([FromQuery] bool incluirDesabilitados, CancellationToken ct)
        {
            var lista = await _service.ListarAsync(incluirDesabilitados, ct);
            var dtos = lista.Select(x => new TipoPerguntaDto
            {
                tipoperguntaid = x.Tipoperguntaid,
                tipopergunta = x.Tipopergunta1 ?? string.Empty,
                desabilitado = x.Desabilitado
            });
            return Ok(dtos);
        }
    }
}
