using Microsoft.AspNetCore.Mvc;
using FASTSURVEY.Services;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerguntasController : ControllerBase
    {
        private readonly ServicePerguntas _servicePerguntas;

        // Injeta a dependência do ServicePerguntas
        public PerguntasController(ServicePerguntas servicePerguntas)
        {
            _servicePerguntas = servicePerguntas;
        }

        // GET: api/perguntas
        [HttpGet]
        public async Task<IActionResult> GetPerguntas()
        {
            try
            {
                var perguntas = await _servicePerguntas.ListarTodasPerguntasAsync();
                if (perguntas == null || perguntas.Count == 0)
                {
                    return NotFound("Nenhuma pergunta encontrada.");
                }

                return Ok(perguntas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar perguntas: {ex.Message}");
            }
        }

        // GET: api/perguntas/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPergunta(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var pergunta = await _servicePerguntas.BuscarPerguntaPorIdAsync(id);
                if (pergunta == null)
                {
                    return NotFound("Pergunta não encontrada.");
                }

                return Ok(pergunta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar pergunta: {ex.Message}");
            }
        }

        // POST: api/perguntas
        [HttpPost]
        public async Task<IActionResult> CreatePergunta([FromBody] perguntas pergunta)
        {
            if (pergunta == null || string.IsNullOrWhiteSpace(pergunta.texto))
            {
                return BadRequest("Dados da pergunta são inválidos.");
            }

            try
            {
                var novaPergunta = await _servicePerguntas.CadastrarPerguntaAsync(pergunta);
                return CreatedAtAction(nameof(GetPergunta), new { id = novaPergunta.perguntaid }, novaPergunta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar pergunta: {ex.Message}");
            }
        }

        // PUT: api/perguntas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePergunta(int id, [FromBody] perguntas pergunta)
        {
            if (id <= 0 || pergunta == null || id != pergunta.perguntaid)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var existingPergunta = await _servicePerguntas.BuscarPerguntaPorIdAsync(id);
                if (existingPergunta == null)
                {
                    return NotFound("Pergunta não encontrada.");
                }

                await _servicePerguntas.AtualizarPerguntaAsync(pergunta);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar pergunta: {ex.Message}");
            }
        }

        // DELETE: api/perguntas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePergunta(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            try
            {
                var pergunta = await _servicePerguntas.BuscarPerguntaPorIdAsync(id);
                if (pergunta == null)
                {
                    return NotFound("Pergunta não encontrada.");
                }

                await _servicePerguntas.ExcluirPerguntaAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir pergunta: {ex.Message}");
            }
        }
    }
}
