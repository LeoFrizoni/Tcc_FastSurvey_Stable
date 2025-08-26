// FASTSURVEY/Controllers/TipoPerguntaController.cs
#nullable enable
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Services.Tipos;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPerguntaController : ControllerBase
    {
        private readonly ITipoPerguntaService _service;

        public TipoPerguntaController(ITipoPerguntaService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista tipos de pergunta. Quando incluirDesabilitados=false (padrão), retorna apenas ativos.
        /// </summary>
        /// <param name="incluirDesabilitados">Se true, inclui também os desabilitados.</param>
        [HttpGet]
        [ProducesResponseType(
            typeof(IReadOnlyList<FASTSURVEY.Dtos.Tipos.TipoPerguntaCatalogDto>),
            200
        )]
        public async Task<IActionResult> Listar(
            [FromQuery] bool incluirDesabilitados = false,
            CancellationToken ct = default
        )
        {
            var data = await _service.ListarAsync(incluirDesabilitados, ct);
            return Ok(data);
        }

        /// <summary>
        /// Atalhos para apenas ativos (equivalente a incluirDesabilitados=false).
        /// </summary>
        [HttpGet("ativos")]
        [ProducesResponseType(
            typeof(IReadOnlyList<FASTSURVEY.Dtos.Tipos.TipoPerguntaCatalogDto>),
            200
        )]
        public async Task<IActionResult> ListarApenasAtivos(CancellationToken ct = default)
        {
            var data = await _service.ListarAsync(incluirDesabilitados: false, ct);
            return Ok(data);
        }
    }
}
