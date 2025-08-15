using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoUsuarioController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryTipoUsuario _repository;

        public TipoUsuarioController(FastSurveyContext context)
        {
            _context = context;
            _repository = new RepositoryTipoUsuario(_context);
        }

        [HttpPost("Cadastrar")]
        public IActionResult Post([FromBody] tipousuario tipo)
        {
            if (tipo == null || string.IsNullOrWhiteSpace(tipo.tipousuario1))
            {
                return BadRequest("O nome do tipo de usuário é obrigatório.");
            }

            _repository.Incluir(tipo);
            return Ok(new
            {
                mensagem = "Tipo de usuário cadastrado com sucesso.",
                tipo = tipo
            });
        }

        [HttpGet("Listar")]
        public IActionResult Get()
        {
            var tipos = _repository.SelecionarTodos();
            if (tipos == null || !tipos.Any())
            {
                return NotFound("Nenhum tipo de usuário encontrado.");
            }

            return Ok(tipos);
        }

        [HttpGet("Selecionar/{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            var tipo = _repository.SelecionarChave(id);
            if (tipo == null)
            {
                return NotFound("Tipo de usuário não encontrado.");
            }

            return Ok(tipo);
        }

        [HttpPut("Alterar/{id}")]
        public IActionResult Put(int id, [FromBody] tipousuario tipo)
        {
            if (id <= 0 || tipo == null || string.IsNullOrWhiteSpace(tipo.tipousuario1))
            {
                return BadRequest("Dados inválidos para alteração.");
            }

            var tipoExistente = _repository.SelecionarChave(id);
            if (tipoExistente == null)
            {
                return NotFound("Tipo de usuário não encontrado.");
            }

            tipo.usuarioid = id;
            _repository.Alterar(tipo);
            return Ok("Tipo de usuário alterado com sucesso.");
        }

        [HttpDelete("Excluir/{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            var tipo = _repository.SelecionarChave(id);
            if (tipo == null)
            {
                return NotFound("Tipo de usuário não encontrado.");
            }

            _repository.Excluir(tipo);
            return Ok("Tipo de usuário excluído com sucesso.");
        }
    }
}
