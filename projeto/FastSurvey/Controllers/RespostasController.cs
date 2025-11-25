using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Resposta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class RespostasController : ControllerBase
    {
        private readonly IRespostaService _svc;

        public RespostasController(IRespostaService svc) => _svc = svc;

        /// <summary>Cria uma resposta discursiva para uma pergunta.</summary>
        [HttpPost("discursiva")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(RespostaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarDiscursiva(
            [FromBody] CriarRespostaDiscursivaRequest req,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entity = await _svc.CriarDiscursivaAsync(req, ct);
                var dto = new RespostaDto
                {
                    RespostaId = entity.RespostaId,
                    PerguntaId = entity.PerguntaId,
                    Texto = entity.Texto,
                    DataResposta = entity.DataResposta,
                    RespondidaEm = entity.RespondidaEm,
                    Opcoes = new(),
                    RespostaAnonima = entity.RespostaAnonima,
                    SessaoId = entity.SessaoId,
                    SessaoCodigo = null,
                    ParticipanteId = entity.ParticipanteId,
                    ParticipanteNome = entity.Participante?.NomeParticipante,
                };
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Erro ao salvar resposta no banco." }
                );
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Erro inesperado ao criar resposta." }
                );
            }
        }

        /// <summary>Cria uma resposta de opções (objetiva/múltipla).</summary>
        [HttpPost("opcoes")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(RespostaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarOpcoes(
            [FromBody] CriarRespostaOpcoesRequest req,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var entity = await _svc.CriarOpcoesAsync(req, ct);
                var dto = new RespostaDto
                {
                    RespostaId = entity.RespostaId,
                    PerguntaId = entity.PerguntaId,
                    Texto = entity.Texto,
                    DataResposta = entity.DataResposta,
                    RespondidaEm = entity.RespondidaEm,
                    Opcoes = entity.Opcao?.Select(o => o.OpcaoId).ToList() ?? new(),
                    RespostaAnonima = entity.RespostaAnonima,
                    SessaoId = entity.SessaoId,
                    SessaoCodigo = null,
                    ParticipanteId = entity.ParticipanteId,
                    ParticipanteNome = entity.Participante?.NomeParticipante,
                };
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Erro ao salvar resposta no banco." }
                );
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Erro inesperado ao criar resposta." }
                );
            }
        }

        /// <summary>Envia um conjunto completo de respostas para uma pesquisa.</summary>
        [HttpPost("{pesquisaId:int}/completa")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> EnviarRespostaCompleta(
            [FromRoute] int pesquisaId,
            [FromBody] EnviarRespostaCompletaRequest request,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var respostaId = await _svc.EnviarRespostaCompletaAsync(pesquisaId, request, ct);

                // service retorna 0 quando nenhuma resposta foi criada
                if (respostaId == 0)
                    return BadRequest(new { error = "Nenhuma resposta foi criada." });

                return CreatedAtAction(
                    nameof(ObterResposta),
                    new { id = respostaId },
                    new { id = respostaId }
                );
            }
            catch (InvalidOperationException ex)
            {
                // ex.: pesquisa não encontrada
                return BadRequest(new { error = ex.Message });
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Erro inesperado ao enviar resposta." }
                );
            }
        }

        /// <summary>Obtém uma resposta pelo id.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RespostaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RespostaDto>> ObterResposta(
            [FromRoute] int id,
            CancellationToken ct = default
        )
        {
            var resposta = await _svc.ObterPorIdAsync(id, ct);
            return resposta is null ? NotFound() : Ok(resposta);
        }

        /// <summary>Lista respostas de uma pesquisa.</summary>
        [HttpGet("pesquisa/{pesquisaId:int}")]
        [ProducesResponseType(typeof(IEnumerable<RespostaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RespostaDto>>> ListarPorPesquisa(
            [FromRoute] int pesquisaId,
            CancellationToken ct = default
        )
        {
            var respostas = await _svc.ListarPorPesquisaAsync(pesquisaId, ct);
            return Ok(respostas);
        }

        /// <summary>Obtém analytics básicos de uma pesquisa.</summary>
        [HttpGet("{pesquisaId:int}/analytics")]
        [ProducesResponseType(typeof(AnalyticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ObterAnalytics(
            [FromRoute] int pesquisaId,
            [FromQuery] EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var analytics = await _svc.ObterAnalyticsAsync(pesquisaId, request, ct);
            return analytics is null ? NotFound() : Ok(analytics);
        }

        /// <summary>Valida um payload de respostas para a pesquisa.</summary>
        [HttpPost("{pesquisaId:int}/validar")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ValidacaoRespostaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ValidarResposta(
            [FromRoute] int pesquisaId,
            [FromBody] ValidarRespostaRequest request,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var validacao = await _svc.ValidarRespostaAsync(pesquisaId, request, ct);
            return Ok(validacao);
        }

        /// <summary>Retorna o total de respostas de uma pesquisa.</summary>
        [HttpGet("{pesquisaId:int}/total")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> ObterTotalRespostas(
            [FromRoute] int pesquisaId,
            CancellationToken ct = default
        )
        {
            var total = await _svc.ObterTotalRespostasAsync(pesquisaId, ct);
            return Ok(new { total });
        }

        /// <summary>Exclui uma resposta.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ExcluirResposta(
            [FromRoute] int id,
            CancellationToken ct = default
        )
        {
            var sucesso = await _svc.ExcluirAsync(id, ct);
            return sucesso ? NoContent() : NotFound();
        }
    }
}
