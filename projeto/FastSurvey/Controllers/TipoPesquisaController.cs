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

        /// <summary>Cadastra um novo tipo de pesquisa.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(TipoPesquisaCatalogDto), 201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cadastrar([FromBody] CadastrarTipoPesquisaRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TipoPesquisa))
                    return BadRequest(new { message = "Nome do tipo de pesquisa é obrigatório." });

                var novo = await _service.CadastrarAsync(request.TipoPesquisa, ct);
                
                // Limpa o cache
                _cache.Remove("tipos_pesquisa:list:false");
                _cache.Remove("tipos_pesquisa:list:true");
                
                return CreatedAtAction(nameof(Get), new { id = novo.TipoPesquisaId }, novo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar tipo de pesquisa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Altera um tipo de pesquisa existente.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Alterar(int id, [FromBody] AlterarTipoPesquisaRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TipoPesquisa))
                    return BadRequest(new { message = "Nome do tipo de pesquisa é obrigatório." });

                var sucesso = await _service.AlterarAsync(id, request.TipoPesquisa, ct);
                if (!sucesso)
                    return NotFound(new { message = "Tipo de pesquisa não encontrado." });

                // Limpa o cache
                _cache.Remove("tipos_pesquisa:list:false");
                _cache.Remove("tipos_pesquisa:list:true");

                return Ok(new { message = "Tipo de pesquisa alterado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar tipo de pesquisa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Exclui um tipo de pesquisa.</summary>
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
                    return NotFound(new { message = "Tipo de pesquisa não encontrado." });

                // Limpa o cache
                _cache.Remove("tipos_pesquisa:list:false");
                _cache.Remove("tipos_pesquisa:list:true");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de pesquisa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
