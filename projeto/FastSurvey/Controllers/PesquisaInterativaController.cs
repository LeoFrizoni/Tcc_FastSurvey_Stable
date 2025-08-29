#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.PesquisaInterativa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/PesquisaInterativa
    [Produces("application/json")]
    public class PesquisaInterativaController : ControllerBase
    {
        private readonly IPesquisaInterativaService _service;
        private readonly ILogger<PesquisaInterativaController> _logger;

        public PesquisaInterativaController(
            IPesquisaInterativaService service,
            ILogger<PesquisaInterativaController> logger
        )
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Inicia uma nova sessão de pesquisa interativa.</summary>
        [HttpPost("iniciar")]
        [Authorize]
        [ProducesResponseType(typeof(SessaoInterativaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> IniciarSessao(
            [FromBody] PesquisaInterativaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.IniciarSessaoAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar sessão de pesquisa interativa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Permite que um participante entre em uma sessão existente.</summary>
        [HttpPost("entrar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ParticipanteResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> EntrarSessao(
            [FromBody] EntrarSessaoRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.EntrarSessaoAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao entrar na sessão {Codigo}", request.CodigoAcesso);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Obtém informações de uma sessão pelo código de acesso.</summary>
        [HttpGet("sessao/{codigo}")]
        public async Task<IActionResult> ObterSessao(
            [FromRoute] string codigo,
            CancellationToken ct
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                    return BadRequest(new { message = "Código de acesso é obrigatório" });

                var result = await _service.ObterSessaoAsync(codigo, ct);
                if (!result.Success)
                    return NotFound(new { message = "Sessão não encontrada" });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão com código {Codigo}", codigo);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Registra a resposta de um participante a uma pergunta.</summary>
        [HttpPost("responder")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RespostaInterativaResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ResponderPergunta(
            [FromBody] RespostaInterativaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.ResponderPerguntaAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar resposta do participante {ParticipanteId}", request.ParticipanteId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Avança para a próxima pergunta na sessão.</summary>
        [HttpPost("avancar")]
        [Authorize]
        [ProducesResponseType(typeof(PerguntaAtualResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AvancarPergunta(
            [FromBody] AvancarPerguntaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.AvancarPerguntaAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao avançar pergunta na sessão {SessaoId}", request.SessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Volta para a pergunta anterior na sessão.</summary>
        [HttpPost("voltar")]
        [Authorize]
        [ProducesResponseType(typeof(PerguntaAtualResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> VoltarPergunta(
            [FromBody] VoltarPerguntaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.VoltarPerguntaAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao voltar pergunta na sessão {SessaoId}", request.SessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Vai para uma pergunta específica na sessão.</summary>
        [HttpPost("ir-para")]
        [Authorize]
        [ProducesResponseType(typeof(PerguntaAtualResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> IrParaPergunta(
            [FromBody] IrParaPerguntaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.IrParaPerguntaAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ir para pergunta {PerguntaId} na sessão {SessaoId}", request.PerguntaId, request.SessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Ativa ou desativa a pergunta atual para respostas.</summary>
        [HttpPost("ativar-pergunta")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AtivarPergunta(
            [FromBody] AtivarPerguntaRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.AtivarPerguntaAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { 
                    message = request.Ativar ? "Pergunta ativada" : "Pergunta desativada",
                    ativa = result.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar/desativar pergunta na sessão {SessaoId}", request.SessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Obtém resultados em tempo real de uma sessão.</summary>
        [HttpGet("resultados/{sessaoId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObterResultadosTempoReal(
            [FromRoute] string sessaoId,
            CancellationToken ct
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessaoId))
                    return BadRequest(new { message = "ID da sessão é obrigatório" });

                var result = await _service.ObterResultadosTempoRealAsync(sessaoId, ct);
                if (!result.Success)
                    return NotFound(new { message = "Resultados não encontrados" });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resultados da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Finaliza uma sessão de pesquisa interativa.</summary>
        [HttpPost("finalizar")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> FinalizarSessao(
            [FromBody] FinalizarSessaoRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.FinalizarSessaoAsync(request, ct);
                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = "Sessão finalizada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar sessão {SessaoId}", request.SessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Obtém o status atual de uma sessão.</summary>
        [HttpGet("status/{sessaoId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObterStatusSessao(
            [FromRoute] string sessaoId,
            CancellationToken ct
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessaoId))
                    return BadRequest(new { message = "ID da sessão é obrigatório" });

                var result = await _service.ObterStatusSessaoAsync(sessaoId, ct);
                if (!result.Success)
                    return NotFound(new { message = "Sessão não encontrada" });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter status da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Obtém lista de participantes de uma sessão.</summary>
        [HttpGet("participantes/{sessaoId}")]
        [Authorize]
        [ProducesResponseType(typeof(List<ParticipanteResponse>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObterParticipantes(
            [FromRoute] string sessaoId,
            CancellationToken ct
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessaoId))
                    return BadRequest(new { message = "ID da sessão é obrigatório" });

                var result = await _service.ObterParticipantesAsync(sessaoId, ct);
                if (!result.Success)
                    return NotFound(new { message = "Sessão não encontrada" });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter participantes da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
