#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.ParticipantesSessao;
using FASTSURVEY.Services.ParticipantesSessao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ParticipanteSessaoController : BaseController
    {
        private readonly IParticipantesSessaoService _service;
        private readonly ILogger<ParticipanteSessaoController> _logger;

        public ParticipanteSessaoController(
            IParticipantesSessaoService service,
            ILogger<ParticipanteSessaoController> logger
        )
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("entrar")]
        [ProducesResponseType(typeof(ParticipanteResponse), 201)]
        public async Task<IActionResult> RegistrarEntrada(
            [FromBody] ParticipanteRequest request,
            CancellationToken ct
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var participante = await _service.RegistrarEntradaAsync(request, ct);
                return CreatedAtAction(nameof(ObterPorId), new { id = participante.ParticipanteId }, participante);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Tentativa de registrar entrada de participante duplicado: {Nome}", request.NomeParticipante);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar entrada do participante");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ParticipanteResponse), 200)]
        public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
        {
            try
            {
                var participante = await _service.ObterParticipanteAsync(id, ct);
                if (participante == null)
                    return NotFound(new { message = "Participante não encontrado" });

                return Ok(participante);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter participante com ID {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("sessao/{sessaoId}")]
        [ProducesResponseType(typeof(List<ParticipanteResponse>), 200)]
        public async Task<IActionResult> ObterParticipantesPorSessao(string sessaoId, CancellationToken ct)
        {
            try
            {
                var participantes = await _service.ObterParticipantesPorSessaoAsync(sessaoId, ct);
                return Ok(participantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar participantes da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }



        [HttpPut("{id:int}/sair")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MarcarSaida(int id, CancellationToken ct)
        {
            try
            {
                var sucesso = await _service.MarcarSaidaAsync(id, ct);
                if (!sucesso)
                    return NotFound(new { message = "Participante não encontrado" });

                return Ok(new { message = "Saída registrada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar saída do participante {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }



        /// <summary>Lista participantes ativos de uma sessão.</summary>
        [HttpGet("sessao/{sessaoId}/ativos")]
        [ProducesResponseType(typeof(List<ParticipanteResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObterParticipantesAtivos(string sessaoId, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessaoId))
                    return BadRequest(new { message = "ID da sessão é obrigatório" });

                var participantes = await _service.ObterParticipantesAtivosAsync(sessaoId, ct);
                return Ok(participantes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar participantes ativos da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>Conta participantes ativos de uma sessão.</summary>
        [HttpGet("sessao/{sessaoId}/contar-ativos")]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ContarParticipantesAtivos(string sessaoId, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessaoId))
                    return BadRequest(new { message = "ID da sessão é obrigatório" });

                var count = await _service.ContarParticipantesAtivosAsync(sessaoId, ct);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar participantes ativos da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }


    }
}
