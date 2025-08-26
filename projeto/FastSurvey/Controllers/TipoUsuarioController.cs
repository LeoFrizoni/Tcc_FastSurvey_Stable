// FASTSURVEY/Controllers/TipoUsuarioController.cs
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
    [Route("api/[controller]")] // /api/TipoUsuario
    [Produces("application/json")]
    [Authorize] // ajuste conforme auth
    public class TipoUsuarioController : ControllerBase
    {
        private readonly ITipoUsuarioService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TipoUsuarioController> _logger;

        public TipoUsuarioController(
            ITipoUsuarioService service,
            IMemoryCache cache,
            ILogger<TipoUsuarioController> logger
        )
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>Lista os tipos de usuário (catálogo).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<TipoUsuarioCatalogDto>), 200)]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            try
            {
                const string cacheKey = "tipos_usuario:list";
                if (_cache.TryGetValue(cacheKey, out IReadOnlyList<TipoUsuarioCatalogDto>? cached))
                    return Ok(cached);

                var data = await _service.ListarAsync(ct);

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
                _logger.LogError(ex, "Erro ao listar tipos de usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
