using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Login;
using FASTSURVEY.Services.Pesquisa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly ILoginService _loginService;
        private readonly IPesquisaService _pesquisaService;

        public AdminController(ILoginService loginService, IPesquisaService pesquisaService)
        {
            _loginService = loginService;
            _pesquisaService = pesquisaService;
        }

        // GET: api/admin/usuarios
        [HttpGet("usuarios")]
        [ProducesResponseType(typeof(IEnumerable<PerfilResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ListarUsuarios(CancellationToken ct)
        {
            // Verificar se é admin (TipoUsuarioId = 15)
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var usuarios = await _loginService.ListarTodosUsuariosAsync(ct);
            return Ok(usuarios);
        }

        // GET: api/admin/usuarios/{id}
        [HttpGet("usuarios/{id:int}")]
        [ProducesResponseType(typeof(PerfilResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ObterUsuario([FromRoute] int id, CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var usuario = await _loginService.ObterPerfilAsync(id, ct);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // PUT: api/admin/usuarios/{id}
        [HttpPut("usuarios/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AtualizarUsuario(
            [FromRoute] int id,
            [FromBody] AtualizarPerfilRequest request,
            CancellationToken ct
        )
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var sucesso = await _loginService.AtualizarPerfilAsync(id, request, ct);
            if (!sucesso)
                return NotFound();

            return Ok(new { message = "Usuário atualizado com sucesso!" });
        }

        // DELETE: api/admin/usuarios/{id}
        [HttpDelete("usuarios/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ExcluirUsuario([FromRoute] int id, CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            // Não permitir excluir a si mesmo
            if (id == loginId)
            {
                return BadRequest(new { message = "Não é possível excluir sua própria conta." });
            }

            var sucesso = await _loginService.ExcluirUsuarioAsync(id, ct);
            if (!sucesso)
                return NotFound();

            return NoContent();
        }

        // GET: api/admin/pesquisas
        [HttpGet("pesquisas")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ListarPesquisasAdmin(CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var pesquisas = await _pesquisaService.ListarTodasPesquisasAsync(ct);
            return Ok(pesquisas);
        }

        // GET: api/admin/estatisticas
        [HttpGet("estatisticas")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> ObterEstatisticasGerais(CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var estatisticas = await _pesquisaService.ObterEstatisticasGeraisAsync(ct);
            return Ok(estatisticas);
        }

        // POST: api/admin/usuarios/{id}/ativar
        [HttpPost("usuarios/{id:int}/ativar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AtivarUsuario([FromRoute] int id, CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            var sucesso = await _loginService.AtivarUsuarioAsync(id, ct);
            if (!sucesso)
                return NotFound();

            return Ok(new { message = "Usuário ativado com sucesso!" });
        }

        // POST: api/admin/usuarios/{id}/desativar
        [HttpPost("usuarios/{id:int}/desativar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DesativarUsuario([FromRoute] int id, CancellationToken ct)
        {
            // Verificar se é admin
            var loginId = GetLoginIdFromToken();
            var perfil = await _loginService.ObterPerfilAsync(loginId, ct);

            if (perfil?.TipoUsuarioId != 15)
            {
                return StatusCode(
                    403,
                    new
                    {
                        message = "Acesso negado. Apenas administradores podem acessar este recurso.",
                    }
                );
            }

            // Não permitir desativar a si mesmo
            if (id == loginId)
            {
                return BadRequest(new { message = "Não é possível desativar sua própria conta." });
            }

            var sucesso = await _loginService.DesativarUsuarioAsync(id, ct);
            if (!sucesso)
                return NotFound();

            return Ok(new { message = "Usuário desativado com sucesso!" });
        }
    }
}
