using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoPerguntaController : ControllerBase
    {
        private readonly RepositoryTipoPergunta _repo;

        public TipoPerguntaController(FastSurveyContext context)
        {
            _repo = new RepositoryTipoPergunta(context, true);
        }

        // POST: api/TipoPergunta/AdicionarTipoPergunta
        [HttpPost("AdicionarTipoPergunta")]
        public async Task<IActionResult> Post([FromBody] tipopergunta body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.tipopergunta1))
                return BadRequest("Descrição do tipo de pergunta é obrigatória.");

            try
            {
                var criado = await _repo.IncluirAsync(body);
                // retorna só os campos primitivos
                return CreatedAtAction(nameof(GetPorId),
                    new { id = criado.tipoperguntaid },
                    new { criado.tipoperguntaid, tipopergunta = criado.tipopergunta1, criado.desabilitado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao inserir tipo de pergunta: {ex.Message}");
            }
        }

        // GET: api/TipoPergunta/ListarTipoPergunta
        [HttpGet("ListarTipoPergunta")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var todos = await _repo.SelecionarTodosAsync();
                return Ok(todos.Select(t => new
                {
                    t.tipoperguntaid,
                    tipopergunta = t.tipopergunta1,
                    t.desabilitado
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar tipos de pergunta: {ex.Message}");
            }
        }

        // GET: api/TipoPergunta/SelecionarTipoPerguntaPorId/5
        [HttpGet("SelecionarTipoPerguntaPorId/{id}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var tp = await _repo.SelecionarChaveAsync(id);
                if (tp == null) return NotFound("Tipo de pergunta não encontrado.");

                return Ok(new { tp.tipoperguntaid, tipopergunta = tp.tipopergunta1, tp.desabilitado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao selecionar tipo de pergunta: {ex.Message}");
            }
        }

        // PUT: api/TipoPergunta/AlterarTipoPerguntaPorId/5
        [HttpPut("AlterarTipoPerguntaPorId/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] tipopergunta body)
        {
            if (id <= 0 || body == null || id != body.tipoperguntaid)
                return BadRequest("Dados inválidos.");

            try
            {
                var existente = await _repo.SelecionarChaveAsync(id);
                if (existente == null) return NotFound("Tipo de pergunta não encontrado.");

                existente.tipopergunta1 = body.tipopergunta1?.Trim();
                existente.desabilitado = body.desabilitado;

                await _repo.AlterarAsync(existente);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de pergunta: {ex.Message}");
            }
        }

        // DELETE: api/TipoPergunta/ExcluirTipoPergunta/5
        [HttpDelete("ExcluirTipoPergunta/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var existente = await _repo.SelecionarChaveAsync(id);
                if (existente == null) return NotFound("Tipo de pergunta não encontrado.");

                await _repo.ExcluirAsync(existente);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir tipo de pergunta: {ex.Message}");
            }
        }
    }
}
