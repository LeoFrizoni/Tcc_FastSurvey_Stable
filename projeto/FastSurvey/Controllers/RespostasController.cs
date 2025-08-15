using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using FASTSURVEY.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RespostasController : ControllerBase
    {
        private readonly ServiceRespostas _serviceRespostas;

        public RespostasController(ServiceRespostas serviceRespostas)
        {
            _serviceRespostas = serviceRespostas;
        }

        // POST: api/Respostas
        [HttpPost]
        public async Task<IActionResult> CadastrarResposta([FromBody] respostas resposta)
        {
            if (resposta == null || string.IsNullOrWhiteSpace(resposta.texto))
            {
                return BadRequest("Dados da resposta são inválidos.");
            }

            try
            {
                var novaResposta = await _serviceRespostas.CadastrarRespostaAsync(resposta);
                return CreatedAtAction(nameof(ObterResposta), new { id = novaResposta.respostaid }, novaResposta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar resposta: {ex.Message}");
            }
        }

        // GET: api/Respostas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObterResposta(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var resposta = await _serviceRespostas.ObterRespostaPorIdAsync(id);
                if (resposta == null)
                {
                    return NotFound("Resposta não encontrada.");
                }

                return Ok(resposta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar resposta: {ex.Message}");
            }
        }

        // GET: api/Respostas/pesquisa/{pesquisaId}
        [HttpGet("pesquisa/{pesquisaId}")]
        public async Task<IActionResult> ObterRespostasPorPesquisa(int pesquisaId)
        {
            if (pesquisaId <= 0)
            {
                return BadRequest("ID da pesquisa inválido.");
            }

            try
            {
                var respostas = await _serviceRespostas.ListarTodasRespostasAsync(); // Ajuste necessário para filtrar por pesquisa
                var respostasFiltradas = respostas.Where(r => r.pergunta.pesquisaid == pesquisaId).ToList();

                if (respostasFiltradas == null || !respostasFiltradas.Any())
                {
                    return NotFound("Nenhuma resposta encontrada para a pesquisa especificada.");
                }

                return Ok(respostasFiltradas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar respostas por pesquisa: {ex.Message}");
            }
        }

        // PUT: api/Respostas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarResposta(int id, [FromBody] respostas resposta)
        {
            if (id <= 0 || resposta == null || id != resposta.respostaid)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var respostaExistente = await _serviceRespostas.ObterRespostaPorIdAsync(id);
                if (respostaExistente == null)
                {
                    return NotFound("Resposta não encontrada.");
                }

                var respostaAtualizada = await _serviceRespostas.AtualizarRespostaAsync(resposta);
                return Ok(respostaAtualizada);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Resposta não encontrada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar resposta: {ex.Message}");
            }
        }

        // DELETE: api/Respostas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirResposta(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var resposta = await _serviceRespostas.ObterRespostaPorIdAsync(id);
                if (resposta == null)
                {
                    return NotFound("Resposta não encontrada.");
                }

                await _serviceRespostas.ExcluirRespostaAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Resposta não encontrada.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir resposta: {ex.Message}");
            }
        }
    }
}
