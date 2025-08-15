using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FASTSURVEY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly RepositoryTipoUsuario _repositoryTipoUsuario;

        public UsuariosController(FastSurveyContext context)
        {
            _repositoryTipoUsuario = new RepositoryTipoUsuario(context);
        }

        [HttpPost("CadastrarTipoUsuario")]
        public async Task<IActionResult> Post([FromForm] tipousuario tipousuario)
        {
            if (tipousuario == null || string.IsNullOrWhiteSpace(tipousuario.tipousuario1))
            {
                return BadRequest("Tipo de usuário inválido.");
            }

            var novoTipo = await _repositoryTipoUsuario.IncluirAsync(tipousuario);
            return CreatedAtAction(nameof(Get), new { id = novoTipo.usuarioid }, novoTipo);
        }

        [HttpGet("ListarTiposUsuario")]
        public async Task<IActionResult> Get()
        {
            var tipos = await _repositoryTipoUsuario.SelecionarTodosAsync();
            if (tipos == null || !tipos.Any())
            {
                return NotFound("Nenhum tipo de usuário encontrado.");
            }
            return Ok(tipos);
        }

        [HttpGet("SelecionarTipoUsuario/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            var tipo = await _repositoryTipoUsuario.SelecionarChaveAsync(id);
            if (tipo == null)
            {
                return NotFound("Tipo de usuário não encontrado.");
            }

            return Ok(tipo);
        }

        [HttpPut("AlterarTipoUsuarioPorId/{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] tipousuario tipousuario)
        {
            if (id <= 0 || tipousuario == null || id != tipousuario.usuarioid)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var tipoExistente = await _repositoryTipoUsuario.SelecionarChaveAsync(id);
                if (tipoExistente == null)
                {
                    return NotFound("Tipo de usuário não encontrado.");
                }

                tipoExistente.tipousuario1 = tipousuario.tipousuario1;
                await _repositoryTipoUsuario.AlterarAsync(tipoExistente);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar tipo de usuário: {ex.Message}");
            }
        }

        [HttpDelete("ExcluirTipoUsuario/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID inválido.");
            }

            var tipo = await _repositoryTipoUsuario.SelecionarChaveAsync(id);
            if (tipo == null)
            {
                return NotFound("Tipo de usuário não encontrado.");
            }

            await _repositoryTipoUsuario.ExcluirAsync(tipo);
            return Ok($"Tipo de usuário com ID {id} excluído com sucesso.");
        }
    }
}
