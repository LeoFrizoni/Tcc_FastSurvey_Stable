using FASTSURVEY.Dtos.Respostas;
using FASTSURVEY.Services.Pesquisa;
using FASTSURVEY.Services.Resposta;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ResponderController : ControllerBase
    {
        private readonly IPesquisaService _pesquisaService;
        private readonly IRespostaService _respostaService;

        public ResponderController(
            IPesquisaService pesquisaService,
            IRespostaService respostaService
        )
        {
            _pesquisaService = pesquisaService;
            _respostaService = respostaService;
        }

        /// <summary>Obtém os dados da pesquisa pública pelo slug.</summary>
        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<ActionResult> ObterPesquisa([FromRoute] string slug, CancellationToken ct)
        {
            var pesquisa = await _pesquisaService.ObterPorSlugAsync(slug, ct);
            if (pesquisa == null)
                return NotFound();

            if (!pesquisa.Ativa)
                return StatusCode(403, new { message = "Pesquisa não está ativa" });

            if (
                pesquisa.TemLimitadorTempo
                && pesquisa.DataFechamento.HasValue
                && pesquisa.DataFechamento.Value < DateTime.UtcNow
            )
            {
                return StatusCode(
                    410,
                    new { message = "Pesquisa expirada", dataExpiracao = pesquisa.DataFechamento }
                );
            }

            if (pesquisa.LimiteRespostas.HasValue)
            {
                var totalRespostas = await _respostaService.ObterTotalRespostasAsync(
                    pesquisa.PesquisaId,
                    ct
                );
                if (totalRespostas >= pesquisa.LimiteRespostas.Value)
                {
                    return StatusCode(403, new { message = "Limite de respostas atingido" });
                }
            }

            return Ok(pesquisa);
        }

        /// <summary>Envia uma resposta completa para a pesquisa.</summary>
        [HttpPost("{id:int}/enviar")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<ActionResult> EnviarResposta(
            [FromRoute] int id,
            [FromBody] EnviarRespostaCompletaRequest request,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var pesquisa = await _pesquisaService.ObterPorIdAsync(id, ct);
            if (pesquisa == null)
                return NotFound();

            if (!pesquisa.Ativa)
                return StatusCode(403, new { message = "Pesquisa não está ativa" });

            if (
                pesquisa.TemLimitadorTempo
                && pesquisa.DataFechamento.HasValue
                && pesquisa.DataFechamento.Value < DateTime.UtcNow
            )
            {
                return StatusCode(410, new { message = "Pesquisa expirada" });
            }

            if (pesquisa.LimiteRespostas.HasValue)
            {
                var totalRespostas = await _respostaService.ObterTotalRespostasAsync(id, ct);
                if (totalRespostas >= pesquisa.LimiteRespostas.Value)
                {
                    return StatusCode(403, new { message = "Limite de respostas atingido" });
                }
            }

            if (pesquisa.RequerIdentificacao == true && request.Respondente == null)
            {
                return BadRequest(
                    new { message = "Identificação é obrigatória para esta pesquisa" }
                );
            }

            var validacao = await _respostaService.ValidarRespostaAsync(
                id,
                new ValidarRespostaRequest
                {
                    Respostas = request.Respostas,
                    RespostaAnonima = request.RespostaAnonima,
                },
                ct
            );

            if (!validacao.Valida)
                return BadRequest(new { message = "Resposta inválida", erros = validacao.Erros });

            var respostaId = await _respostaService.EnviarRespostaCompletaAsync(id, request, ct);

            // Aqui idealmente usar o SLUG real da pesquisa
            return CreatedAtAction(
                nameof(ObterPesquisa),
                new { slug = pesquisa.Slug ?? id.ToString() },
                new
                {
                    id = respostaId,
                    message = "Resposta enviada com sucesso!",
                    protocolo = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                }
            );
        }

        /// <summary>Valida um payload de resposta antes do envio.</summary>
        [HttpPost("{id:int}/validar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ValidarResposta(
            [FromRoute] int id,
            [FromBody] ValidarRespostaRequest request,
            CancellationToken ct
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var validacao = await _respostaService.ValidarRespostaAsync(id, request, ct);
            return Ok(validacao);
        }

        /// <summary>Obtém o status resumido de uma pesquisa.</summary>
        [HttpGet("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ObterStatus([FromRoute] int id, CancellationToken ct)
        {
            var status = await _pesquisaService.ObterStatusAsync(id, ct);
            return status is null ? NotFound() : Ok(status);
        }
    }
}
