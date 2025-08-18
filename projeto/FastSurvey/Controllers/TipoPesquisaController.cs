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
    public class TipoPesquisaController : ControllerBase
    {
        private readonly RepositoryTipoPesquisa _repo;

        public TipoPesquisaController(FastSurveyContext context)
        {
            _repo = new RepositoryTipoPesquisa(context, true);
        }

        // POST: api/TipoPesquisa/AdicionarTipoPesquisa
        [HttpPost("AdicionarTipoPesquisa")]
        public async Task<IActionResult> Post([FromBody] tipopesquisa body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.tipopesquisa1))
                return BadRequest("Descrição do tipo de pesquisa é obrigatória.");

            try
            {
                var criado = await _repo.IncluirAsync(body);
                return CreatedAtAction(nameof(GetPorId),
                    new { id = criado.tipopesquisaid },
                    new { criado.tipopesquisaid, tipopesquisa = criado.tipopesquisa1, criado.desabilitado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao inserir tipo de pesquisa: {ex.Message}");
            }
        }

        // GET: api/TipoPesquisa/ListarTipoPesquisa
        [HttpGet("ListarTipoPesquisa")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var todos = await _repo.SelecionarTodosAsync();
                if (todos == null || !todos.Any())
                    return NotFound("Nenhum tipo de pesquisa encontrado.");

                return Ok(todos.Select(t => new
                {
                    t.tipopesquisaid,
                    tipopesquisa = t.tipopesquisa1,
                    t.desabilitado
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar tipos de pesquisa: {ex.Message}");
            }
        }

        // GET: api/TipoPesquisa/SelecionarTipoPesquisaPorId/5
        [HttpGet("SelecionarTipoPesquisaPorId/{id}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var tp = await _repo.SelecionarChaveAsync(id);
                if (tp == null) return NotFound("Tipo de pesquisa não encontrado.");

                return Ok(new { tp.tipopesquisaid, tipopesquisa = tp.tipopesquisa1, tp.desabilitado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao selecionar tipo de pesquisa: {ex.Message}");
            }
        }

        // PUT: api/TipoPesquisa/AlterarTipoPesquisaPorId/5
        [HttpPut("AlterarTipoPesquisaPorId/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] tipopesquisa body)
        {
            if (id <= 0 || body == null || id != body.tipopesquisaid)
                return BadRequest("Dados inválidos.");

            try
            {
                var existente = await _repo.SelecionarChaveAsync(id);
                if (existente == null) return NotFound("Tipo de pesquisa não encontrado.");

                existente.tipopesquisa1 = body.tipopesquisa1?.Trim();
                existente.desabilitado = body.desabilitado;

                await _repo.AlterarAsync(existente);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de pesquisa: {ex.Message}");
            }
        }

        // DELETE: api/TipoPesquisa/ExcluirTipoPesquisa/5
        [HttpDelete("ExcluirTipoPesquisa/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");

            try
            {
                var existente = await _repo.SelecionarChaveAsync(id);
                if (existente == null) return NotFound("Tipo de pesquisa não encontrado.");

                await _repo.ExcluirAsync(existente);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir tipo de pesquisa: {ex.Message}");
            }
        }
    }
}
