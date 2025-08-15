using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoPerguntaController : ControllerBase
    {
        private FastSurveyContext _context;
        private RepositoryTipoPergunta _repositoryTipoPergunta;
        public TipoPerguntaController(FastSurveyContext context)
        {
            _context = context;
            _repositoryTipoPergunta = new RepositoryTipoPergunta(_context, true);
        }

        [HttpPost("AdicionarTipoPergunta")]
        public IActionResult Post([FromForm] tipopergunta tipoPergunta) //FromBody
        {
            if (tipoPergunta == null)
            {
                return BadRequest("Tipo de pergunta não pode ser nulo.");
            }
            try
            {
                _repositoryTipoPergunta.Incluir(tipoPergunta);
                return Ok("ok");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao inserir tipo de pergunta: {ex.Message}");
            }
        }

        [HttpGet("ListarTipoPergunta")]
        public IActionResult Get()
        {
            try
            {
                var tiposPergunta = _repositoryTipoPergunta.SelecionarTodos();
                return Ok(tiposPergunta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar tipos de pergunta: {ex.Message}");
            }
        }

        [HttpGet("SelecionarTipoPerguntaPorId/{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var tipoPergunta = _repositoryTipoPergunta.SelecionarChave(id);
                if (tipoPergunta == null)
                {
                    return NotFound("Tipo de pergunta não encontrado.");
                }
                return Ok(tipoPergunta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao selecionar tipo de pergunta: {ex.Message}");
            }

        }

        [HttpPut("AlterarTipoPerguntaPorId/{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] tipopergunta tipoPergunta)//FromBody
        {
            if (id <= 0 || tipoPergunta == null || id != tipoPergunta.tipoperguntaid)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                // Verifica se o tipo de pergunta existe no banco
                var tipoPerguntaExistente = await _repositoryTipoPergunta.SelecionarChaveAsync(id);
                if (tipoPerguntaExistente == null)
                {
                    return NotFound("Tipo de pergunta não encontrado.");
                }

                // Atualiza o tipo de pergunta com base no ID
                tipoPerguntaExistente.tipopergunta1 = tipoPergunta.tipopergunta1; // Atualize conforme necessário

                // Chama o repositório para persistir as alterações
                await _repositoryTipoPergunta.AlterarAsync(tipoPerguntaExistente);

                return NoContent(); // Retorna 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de pergunta: {ex.Message}");
            }
        }


        [HttpDelete("ExcluirTipoPergunta/{id}")]
        public IActionResult Delete(int id)
        {
            var tipoPergunta = _repositoryTipoPergunta.SelecionarChave(id);
            if (tipoPergunta == null)
            {
                return NotFound("Tipo de pergunta não encontrado.");
            }
            try
            {
                _repositoryTipoPergunta.Excluir(tipoPergunta);
                return Ok("ok");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir tipo de pergunta: {ex.Message}");
            }
        }
    }
}
