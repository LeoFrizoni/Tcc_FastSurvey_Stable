// FASTSURVEY/Controllers/TipoPesquisaController.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Tipos;
using FASTSURVEY.Services.Tipos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ajuste conforme seu auth
    public class TipoPesquisaController : ControllerBase
    {
        private readonly ITipoPesquisaService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TipoPesquisaController> _logger;

        public TipoPesquisaController(
            ITipoPesquisaService service,
            IMemoryCache cache,
            ILogger<TipoPesquisaController> logger
        )
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        // REST padrão: GET /api/TipoPesquisa?incluirDesabilitados=false
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TipoPesquisaCatalogDto>), 200)]
        public async Task<IActionResult> Get(
            [FromQuery] bool incluirDesabilitados = false,
            CancellationToken ct = default
        )
        {
            try
            {
                var cacheKey = $"tipos_pesquisa:list:{incluirDesabilitados}";
                if (_cache.TryGetValue(cacheKey, out IReadOnlyList<TipoPesquisaCatalogDto>? cached))
                    return Ok(cached);

                var data = await _service.ListarAsync(incluirDesabilitados, ct);

                _cache.Set(
                    cacheKey,
                    data,
                    new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                );

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tipos de pesquisa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        // Rota legada para compatibilidade com o front existente:
        // GET /api/TipoPesquisa/ListarTipoPesquisa
        [HttpGet("ListarTipoPesquisa")]
        [ProducesResponseType(typeof(IReadOnlyList<TipoPesquisaCatalogDto>), 200)]
        public Task<IActionResult> ListarTipoPesquisa(CancellationToken ct) =>
            Get(incluirDesabilitados: false, ct);
    }
}
