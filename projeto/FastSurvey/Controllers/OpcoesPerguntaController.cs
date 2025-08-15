using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpcoesPerguntaController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryOpcoesPergunta _repositoryOpcoesPergunta;

        public OpcoesPerguntaController(FastSurveyContext context)
        {
            _context = context;
            _repositoryOpcoesPergunta = new RepositoryOpcoesPergunta(_context);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] opcoespergunta opcao)
        {
            if (opcao == null || string.IsNullOrWhiteSpace(opcao.texto) || opcao.perguntaid <= 0)
                return BadRequest("Dados inválidos para a opção.");

            try
            {
                var novaOpcao = await _repositoryOpcoesPergunta.IncluirAsync(opcao);
                return CreatedAtAction(nameof(GetPorPergunta), new { perguntaId = novaOpcao.perguntaid }, novaOpcao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar opção: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListarTodas()
        {
            try
            {
                var opcoes = await _repositoryOpcoesPergunta.SelecionarTodosAsync();
                if (!opcoes.Any())
                    return NotFound("Nenhuma opção encontrada.");

                return Ok(opcoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar opções: {ex.Message}");
            }
        }

        [HttpGet("Pergunta/{perguntaId}")]
        public async Task<IActionResult> GetPorPergunta(int perguntaId)
        {
            if (perguntaId <= 0)
                return BadRequest("ID de pergunta inválido.");

            try
            {
                var opcoes = (await _repositoryOpcoesPergunta.SelecionarTodosAsync())
                    .Where(o => o.perguntaid == perguntaId)
                    .ToList();

                return Ok(opcoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar opções: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] opcoespergunta opcao)
        {
            if (id <= 0 || opcao == null || id != opcao.opcaoid)
                return BadRequest("Dados inválidos para atualização.");

            try
            {
                var existente = await _repositoryOpcoesPergunta.SelecionarChaveAsync(id);
                if (existente == null)
                    return NotFound("Opção não encontrada.");

                existente.texto = opcao.texto;
                await _repositoryOpcoesPergunta.AlterarAsync(existente);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar opção: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("ID inválido.");

            try
            {
                var opcao = await _repositoryOpcoesPergunta.SelecionarChaveAsync(id);
                if (opcao == null)
                    return NotFound("Opção não encontrada.");

                await _repositoryOpcoesPergunta.ExcluirAsync(opcao);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir opção: {ex.Message}");
            }
        }
    }
}
