using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Data;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _auth;
        private readonly FastSurveyContext _ctx;
        private readonly IConfiguration _cfg;

        public LoginController(ILoginService auth, FastSurveyContext ctx, IConfiguration cfg)
        {
            _auth = auth;
            _ctx = ctx;
            _cfg = cfg;
        }

        [AllowAnonymous]
        [HttpPost("Autenticar")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var resp = await _auth.AutenticarAsync(req, ct);
                return Ok(new
                {
                    id = resp.Id,
                    usuario = resp.Usuario,
                    tipousuarioid = resp.TipoUsuarioId,
                    token = resp.Token
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("Cadastrar")]
        [ProducesResponseType(typeof(CadastrarLoginResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Cadastrar([FromForm] CadastrarLoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var resp = await _auth.CadastrarAsync(req, ct);
                return CreatedAtAction(nameof(Autenticar), new { }, new
                {
                    id = resp.Id,
                    usuario = resp.Usuario,
                    email = resp.Email,
                    tipousuarioid = resp.TipoUsuarioId
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("EsqueciSenha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EsqueciSenha([FromBody] ForgotPasswordRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _auth.SolicitarResetSenhaAsync(req, ct);
                return Ok(new { message = "Se o email existir em nossa base, voc� receber� um link para resetar sua senha." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ResetarSenha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetarSenha([FromBody] ResetPasswordRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _auth.ResetarSenhaAsync(req, ct);
                if (result)
                    return Ok(new { message = "Senha alterada com sucesso!" });
                else
                    return BadRequest(new { message = "Token inv�lido ou expirado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ConfirmarEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmarEmail([FromBody] VerifyEmailRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var result = await _auth.ConfirmarEmailAsync(req, ct);
                if (result)
                    return Ok(new { message = "Email confirmado com sucesso!" });
                else
                    return BadRequest(new { message = "Token inv�lido ou expirado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("Google")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginGoogle([FromBody] ExternalLoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var resp = await _auth.LoginGoogleAsync(req, ct);
                return Ok(new
                {
                    id = resp.Id,
                    usuario = resp.Usuario,
                    tipousuarioid = resp.TipoUsuarioId,
                    token = resp.Token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("Perfil")]
        [ProducesResponseType(typeof(PerfilResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPerfil(CancellationToken ct)
        {
            try
            {
                var loginId = int.Parse(User.FindFirst("loginId")?.Value ?? "0");
                if (loginId <= 0) return Unauthorized();

                var perfil = await _auth.ObterPerfilAsync(loginId, ct);
                if (perfil is null) return NotFound();

                return Ok(new
                {
                    id = perfil.Id,
                    usuario = perfil.Usuario,
                    email = perfil.Email,
                    tipousuarioid = perfil.TipoUsuarioId,
                    avatarUrl = perfil.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Perfil")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AtualizarPerfil([FromBody] object request, CancellationToken ct)
        {
            try
            {
                var loginId = int.Parse(User.FindFirst("loginId")?.Value ?? "0");
                if (loginId <= 0) return Unauthorized();

                // Implementar l�gica de atualiza��o de perfil
                return Ok(new { message = "Perfil atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
