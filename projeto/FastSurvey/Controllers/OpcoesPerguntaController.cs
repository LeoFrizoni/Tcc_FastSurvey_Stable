#nullable enable
using FASTSURVEY.Dtos.Opcoes;
using FASTSURVEY.Services.Opcoes;
using Microsoft.AspNetCore.Mvc;

// ===== ALIAS para casar com a Model do scaffold =====
using OpcaoPergunta = SISTEMA_FASTSURVEY.MODEL.Models.Opcoespergunta;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/opcaopergunta")]
    public class OpcaoPerguntaController : ControllerBase
    {
        private readonly IOpcaoPerguntaService _service;
        public OpcaoPerguntaController(IOpcaoPerguntaService service) => _service = service;

        // -------- Mapper --------
        private static OpcaoPerguntaResponse MapToResponse(OpcaoPergunta e) => new()
        {
            OpcaoId = e.Opcaoid,
            PerguntaId = e.Perguntaid,
            Texto = e.Texto ?? string.Empty,
            Ordem = e.Ordem,
            Ativa = e.Ativa,
            Correta = e.Correta
        };

        // -------- Endpoints --------

        /// <summary>Cria uma op��o vinculada � pergunta (informando PerguntaId no body).</summary>
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] OpcaoPerguntaRequest dto, CancellationToken ct)
        {
            if (dto is null) return BadRequest("Body inv�lido.");
            if ((dto.PerguntaId ?? 0) <= 0)
                return BadRequest("Informe PerguntaId no body ou use a rota /pergunta/{perguntaId}.");

            var result = await _service.CriarAsync(
                perguntaId: dto.PerguntaId.Value,
                texto: dto.Texto,
                correta: dto.Correta,
                ordem: dto.Ordem,
                ativa: dto.Ativa,
                ct: ct
            );

            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Erro ao criar.");

            return Ok(MapToResponse(result.Data!));
        }

        /// <summary>Cria uma op��o pela rota com perguntaId.</summary>
        [HttpPost("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> CriarNaPergunta([FromRoute] int perguntaId, [FromBody] OpcaoPerguntaRequest dto, CancellationToken ct)
        {
            if (dto is null) return BadRequest("Body inv�lido.");

            var result = await _service.CriarAsync(
                perguntaId: perguntaId,
                texto: dto.Texto,
                correta: dto.Correta,
                ordem: dto.Ordem,
                ativa: dto.Ativa,
                ct: ct
            );

            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Erro ao criar.");

            return Ok(MapToResponse(result.Data!));
        }

        /// <summary>Obt�m uma op��o por id.</summary>
        [HttpGet("{opcaoId:int}")]
        public async Task<IActionResult> Obter([FromRoute] int opcaoId, CancellationToken ct)
        {
            var result = await _service.ObterPorIdAsync(opcaoId, ct);
            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Op��o n�o encontrada.");

            return Ok(MapToResponse(result.Data!));
        }

        /// <summary>Lista op��es de uma pergunta.</summary>
        [HttpGet("pergunta/{perguntaId:int}")]
        public async Task<IActionResult> ListarPorPergunta([FromRoute] int perguntaId, CancellationToken ct)
        {
            var result = await _service.ListarPorPerguntaAsync(perguntaId, ct);
            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Pergunta n�o encontrada.");

            var list = result.Data!.Select(MapToResponse).ToList();
            return Ok(list);
        }

        /// <summary>Atualiza parcialmente uma op��o.</summary>
        [HttpPut("{opcaoId:int}")]
        public async Task<IActionResult> Atualizar([FromRoute] int opcaoId, [FromBody] OpcaoPerguntaUpdateRequest dto, CancellationToken ct)
        {
            if (dto is null || dto.OpcaoId != opcaoId)
                return BadRequest("Id da rota difere do body.");

            var result = await _service.AtualizarAsync(opcaoId, e =>
            {
                if (!string.IsNullOrWhiteSpace(dto.Texto)) e.Texto = dto.Texto.Trim();
                if (dto.Correta.HasValue) e.Correta = dto.Correta.Value;
                if (dto.Ordem.HasValue) e.Ordem = dto.Ordem.Value;
                if (dto.Ativa.HasValue) e.Ativa = dto.Ativa.Value;
            }, ct);

            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Op��o n�o encontrada.");

            return Ok(MapToResponse(result.Data!));
        }

        /// <summary>Remove uma op��o (hard delete).</summary>
        [HttpDelete("{opcaoId:int}")]
        public async Task<IActionResult> Remover([FromRoute] int opcaoId, CancellationToken ct)
        {
            var result = await _service.RemoverAsync(opcaoId, ct);
            if (!result.Success)
                return NotFound(result.Errors.FirstOrDefault()?.Message ?? "Op��o n�o encontrada.");

            return Ok(new { success = true });
        }
    }
}
