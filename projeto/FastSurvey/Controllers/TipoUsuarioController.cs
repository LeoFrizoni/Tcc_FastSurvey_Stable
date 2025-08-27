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

        /// <summary>Cadastra um novo tipo de usuário.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(TipoUsuarioCatalogDto), 201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarTipoUsuarioRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TipoUsuario))
                    return BadRequest(new { message = "Nome do tipo de usuário é obrigatório." });

                var novo = await _service.CadastrarAsync(request.TipoUsuario, ct);
                
                // Limpa o cache
                _cache.Remove("tipos_usuario:list");
                
                return CreatedAtAction(nameof(Get), new { id = novo.TipoUsuarioId }, novo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar tipo de usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Altera um tipo de usuário existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Alterar(int id, [FromBody] AlterarTipoUsuarioRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TipoUsuario))
                    return BadRequest(new { message = "Nome do tipo de usuário é obrigatório." });

                var sucesso = await _service.AlterarAsync(id, request.TipoUsuario, ct);
                if (!sucesso)
                    return NotFound(new { message = "Tipo de usuário não encontrado." });

                // Limpa o cache
                _cache.Remove("tipos_usuario:list");

                return Ok(new { message = "Tipo de usuário alterado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar tipo de usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Exclui um tipo de usuário.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Excluir(int id, CancellationToken ct)
        {
            try
            {
                var sucesso = await _service.ExcluirAsync(id, ct);
                if (!sucesso)
                    return NotFound(new { message = "Tipo de usuário não encontrado." });

                // Limpa o cache
                _cache.Remove("tipos_usuario:list");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
