using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;
using SISTEMA_FASTSURVEY.MODEL.Repositories;
using System;

namespace FASTSURVEY.Controllers
{
    public class LoginRequestDto
    {
        public string usuario { get; set; } = "";
        public string senha { get; set; } = "";
    }

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly FastSurveyContext _context;
        private readonly RepositoryLogin _repositoryLogin;

        public LoginController(FastSurveyContext context)
        {
            _context = context;
            _repositoryLogin = new RepositoryLogin(_context);
        }

        [HttpPost("CadastrarLogin")]
        public async Task<IActionResult> Post([FromForm] login login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.usuario) || string.IsNullOrWhiteSpace(login.senha))
                return BadRequest("Dados do login são inválidos ou incompletos.");

            try
            {
                // normaliza
                login.usuario = login.usuario.Trim();
                login.email = login.email?.Trim();
                login.senha = login.senha.Trim();

                var loginCriado = await _repositoryLogin.IncluirAsync(login);
                return CreatedAtAction(nameof(GetPorId), new { id = loginCriado.loginid }, new
                {
                    id = loginCriado.loginid,
                    usuario = loginCriado.usuario,
                    email = loginCriado.email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar login: {ex.Message}");
            }
        }

        [HttpPost("Autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequestDto cred)
        {
            if (cred == null || string.IsNullOrWhiteSpace(cred.usuario) || string.IsNullOrWhiteSpace(cred.senha))
                return BadRequest("Usuário e senha são obrigatórios.");

            var usuarioTrim = cred.usuario.Trim();
            var senhaTrim = cred.senha.Trim();

            try
            {
                // Para Postgres, use EF.Functions.ILike para case-insensitive do usuário
                var loginEncontrado = await _context.login
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l =>
                        EF.Functions.ILike(l.usuario, usuarioTrim) && l.senha == senhaTrim);

                if (loginEncontrado == null)
                    return Unauthorized("Usuário ou senha inválidos.");

                return Ok(new
                {
                    id = loginEncontrado.loginid,
                    usuario = loginEncontrado.usuario,
                    token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()), // token fake de dev
                    tipousuarioid = loginEncontrado.tipousuarioid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao autenticar: {ex.Message}");
            }
        }

        [HttpGet("ListarLogins")]
        public async Task<IActionResult> ListarLogins()
        {
            try
            {
                var logins = await _repositoryLogin.SelecionarTodosAsync();
                if (logins == null || !logins.Any()) return NotFound("Nenhum login encontrado.");
                return Ok(logins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao listar logins: {ex.Message}");
            }
        }

        [HttpGet("SelecionarLoginPorId/{id}")]
        public async Task<IActionResult> GetPorId(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");
            try
            {
                var login = await _repositoryLogin.SelecionarChaveAsync(id);
                if (login == null) return NotFound("Login não encontrado.");
                return Ok(login);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar login: {ex.Message}");
            }
        }

        [HttpPut("AlterarLoginPorId/{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] login login)
        {
            if (id <= 0 || login == null || id != login.loginid) return BadRequest("Dados inválidos.");
            try
            {
                var existing = await _repositoryLogin.SelecionarChaveAsync(id);
                if (existing == null) return NotFound("Login não encontrado.");

                existing.usuario = login.usuario?.Trim();
                existing.senha = login.senha?.Trim();
                existing.email = login.email?.Trim();

                await _repositoryLogin.AlterarAsync(existing);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar login: {ex.Message}");
            }
        }

        [HttpDelete("ExcluirLogin/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("ID inválido.");
            try
            {
                var login = await _repositoryLogin.SelecionarChaveAsync(id);
                if (login == null) return NotFound("Login não encontrado.");
                await _repositoryLogin.ExcluirAsync(login);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir login: {ex.Message}");
            }
        }

        [HttpGet("me/{id}")]
        public async Task<IActionResult> GetUsuarioPorId(int id)
        {
            try
            {
                var login = await _repositoryLogin.SelecionarChaveAsync(id);
                if (login == null) return NotFound("Usuário não encontrado.");
                return Ok(new { id = login.loginid, nome = login.usuario, email = login.email, status = "Ativo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar usuário: {ex.Message}");
            }
        }
    }
}
