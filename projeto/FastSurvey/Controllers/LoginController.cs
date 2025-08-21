// Controllers/LoginController.cs
using FASTSURVEY.Dtos.Auth;
using FASTSURVEY.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _auth;

        public LoginController(ILoginService auth) => _auth = auth;

        /// <summary>
        /// Autentica o usuário e retorna o JWT.
        /// </summary>
        [AllowAnonymous] // login precisa ser público
        [HttpPost("Autenticar")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

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
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Cadastra um novo usuário. (multipart/form-data)
        /// </summary>
        [AllowAnonymous] // cadastro também precisa ser público
        [HttpPost("CadastrarLogin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CadastrarLoginResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cadastrar([FromForm] CadastrarLoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var resp = await _auth.CadastrarAsync(req, ct);
                return CreatedAtAction(nameof(Autenticar), new { usuario = resp.Usuario }, resp);
            }
            catch (DuplicateNameException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Exemplo de rota que exige login (somente usuários autenticados).
        /// </summary>
        [Authorize]
        [HttpGet("Perfil")]
        public IActionResult GetPerfil()
        {
            var loginId = User.FindFirst("loginId")?.Value;
            var nome = User.Identity?.Name;
            var tipoUsuarioId = User.FindFirst("tipoUsuarioId")?.Value;

            return Ok(new
            {
                loginId,
                usuario = nome,
                tipoUsuarioId
            });
        }
    }
}
