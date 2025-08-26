using FASTSURVEY.Dtos.Pastas;
using FASTSURVEY.Services.Pasta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PastasController : BaseController
    {
        private readonly IPastaService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PastasController> _logger;

        public PastasController(
            IPastaService service,
            IMemoryCache cache,
            ILogger<PastasController> logger
        )
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PastaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(CancellationToken ct)
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var cacheKey = $"pastas_user_{loginId}";

                if (
                    _cache.TryGetValue(cacheKey, out List<PastaResponse>? cached)
                    && cached is not null
                )
                    return Ok(cached);

                var result = await _service.ListarAsync(loginId, ct);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                _cache.Set(cacheKey, result, cacheOptions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pastas");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(PastaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Criar(
            [FromBody] CriarPastaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                // Força usar o loginId do token, ignorando o que vier no body
                request.LoginId = GetLoginIdFromToken();

                var created = await _service.CriarAsync(request, ct);

                _cache.Remove($"pastas_user_{request.LoginId}");
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // conflito de nome
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pasta");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPut("{pastaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Renomear(
            [FromRoute] int pastaId,
            [FromBody] RenomearPastaRequest req,
            CancellationToken ct
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var ok = await _service.RenomearAsync(pastaId, loginId, req.NovoNome, ct);
                if (!ok)
                    return BadRequest(
                        new
                        {
                            message = "Não foi possível renomear a pasta (nome inválido ou em uso).",
                        }
                    );

                _cache.Remove($"pastas_user_{loginId}");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renomear pasta {PastaId}", pastaId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpDelete("{pastaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Excluir([FromRoute] int pastaId, CancellationToken ct)
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var ok = await _service.ExcluirAsync(pastaId, loginId, ct);
                if (!ok)
                    return NotFound(new { message = "Pasta não encontrada." });

                _cache.Remove($"pastas_user_{loginId}");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pasta {PastaId}", pastaId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
