// FASTSURVEY/Controllers/OpcaoPerguntaController.cs
#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Services.Opcoes;
using FASTSURVEY.Services.Result;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/opcaopergunta")]
    public class OpcaoPerguntaController : ControllerBase
    {
        private readonly IOpcaoPerguntaService _service;

        public OpcaoPerguntaController(IOpcaoPerguntaService service) => _service = service;

        private IActionResult ToActionResult<T>(ServiceResult<T> result)
        {
            if (result == null)
                return StatusCode(500, "Erro inesperado.");
            if (result.Success)
                return Ok(result.Data);
            return BadRequest(result.Errors);
        }

        /// <summary>Cria uma opção vinculada à pergunta (informando PerguntaId no body).</summary>
        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromBody] OpcaoPerguntaRequest dto,
            CancellationToken ct
        )
        {
            if (dto is null)
                return BadRequest("Body inválido.");
            if ((dto.PerguntaId ?? 0) <= 0)
                return BadRequest(
                    "Informe PerguntaId no body ou use a rota /pergunta/{perguntaId}."
                );

            var result = await _service.CriarAsync(dto.PerguntaId.Value, dto, ct);
            return ToActionResult(result);
        }

        /// <summary>Cria uma opção pela rota com perguntaId.</summary>
        [HttpPost("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> CriarNaPergunta(
            [FromRoute] int perguntaId,
            [FromBody] OpcaoPerguntaRequest dto,
            CancellationToken ct
        )
        {
            if (dto is null)
                return BadRequest("Body inválido.");
            var result = await _service.CriarAsync(perguntaId, dto, ct);
            return ToActionResult(result);
        }

        /// <summary>Obtém uma opção por id.</summary>
        [HttpGet("{opcaoId:int}")]
        public async Task<IActionResult> Obter([FromRoute] int opcaoId, CancellationToken ct)
        {
            var result = await _service.ObterPorIdAsync(opcaoId, ct);
            return ToActionResult(result);
        }

        /// <summary>Lista opções de uma pergunta.</summary>
        [HttpGet("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ListarPorPergunta(
            [FromRoute] int perguntaId,
            CancellationToken ct
        )
        {
            var result = await _service.ListarPorPerguntaAsync(perguntaId, ct);
            return ToActionResult(result);
        }

        /// <summary>Atualiza parcialmente uma opção.</summary>
        [HttpPut("{opcaoId:int}")]
        public async Task<IActionResult> Atualizar(
            [FromRoute] int opcaoId,
            [FromBody] OpcaoPerguntaUpdateRequest dto,
            CancellationToken ct
        )
        {
            if (dto is null || dto.OpcaoId != opcaoId)
                return BadRequest("Id da rota difere do body.");

            var result = await _service.AtualizarAsync(dto, ct);
            return ToActionResult(result);
        }

        /// <summary>Remove uma opção (hard delete).</summary>
        [HttpDelete("{opcaoId:int}")]
        public async Task<IActionResult> Remover([FromRoute] int opcaoId, CancellationToken ct)
        {
            var result = await _service.RemoverAsync(opcaoId, ct);
            return ToActionResult(result);
        }
    }
}
