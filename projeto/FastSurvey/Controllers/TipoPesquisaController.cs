using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoPesquisaController : ControllerBase
    {
        private FastSurveyContext _context;
        private RepositoryTipoPesquisa _repositoryTipoPesquisa;

        public TipoPesquisaController(FastSurveyContext context)
        {
            _context = context;
            _repositoryTipoPesquisa = new RepositoryTipoPesquisa(_context, true);
        }

        [HttpPost("Adicionar TipoPesquisa")]
        public IActionResult AdicionarTipoPesquisa([FromForm] tipopesquisa tipoPesquisa)//FromBody
        {
            if (tipoPesquisa == null)
            {
                return BadRequest("Tipo de pesquisa inválido.");
            }
            _repositoryTipoPesquisa.Incluir(tipoPesquisa);
            return Ok(tipoPesquisa);
        }

        [HttpGet("ListarTipoPesquisa")]
        public IActionResult ListarTipoPesquisa()
        {
            var tiposPesquisa = _repositoryTipoPesquisa.SelecionarTodos();
            if (tiposPesquisa == null || !tiposPesquisa.Any())
            {
                return NotFound("Nenhum tipo de pesquisa encontrado.");
            }
            return Ok(tiposPesquisa);
        }

        [HttpGet("SelecionarTipoPesquisaPorId/{id}")]
        public IActionResult SelecionarTipoPesquisa(int id)
        {
            var tipoPesquisa = _repositoryTipoPesquisa.SelecionarChave(id);
            if (tipoPesquisa == null)
            {
                return NotFound("Tipo de pesquisa não encontrado.");
            }
            return Ok(tipoPesquisa);
        }

        [HttpPut("AlterarTipoPesquisaPorId/{id}")]
        public IActionResult AlterarTipoPesquisa(int id, [FromForm] tipopesquisa tipoPesquisa)//FromBody
        {
            if (tipoPesquisa == null)
            {
                return BadRequest("Tipo de pesquisa inválido.");
            }
            var tipoPesquisaExistente = _repositoryTipoPesquisa.SelecionarChave(id);
            if (tipoPesquisaExistente == null)
            {
                return NotFound("Tipo de pesquisa não encontrado.");
            }
            _repositoryTipoPesquisa.Alterar(tipoPesquisa);
            return Ok(tipoPesquisa);
        }

        [HttpDelete("ExcluirTipoPesquisa/{id}")]
        public IActionResult ExcluirTipoPesquisa(int id)
        {
            var tipoPesquisa = _repositoryTipoPesquisa.SelecionarChave(id);
            if (tipoPesquisa == null)
            {
                return NotFound("Tipo de pesquisa não encontrado.");
            }
            _repositoryTipoPesquisa.Excluir(tipoPesquisa);
            return Ok("Tipo de pesquisa excluído com sucesso.");
        }
    }
}
