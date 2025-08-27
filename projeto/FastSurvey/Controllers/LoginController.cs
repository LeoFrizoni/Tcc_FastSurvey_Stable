#nullable enable
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Login;
using FASTSURVEY.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoginController : BaseController
    {
        private readonly ILoginService _loginService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILoginService loginService, ILogger<LoginController> logger)
        {
            _loginService = loginService;
            _logger = logger;
        }

        // ======================== AUTENTICAÇÃO ========================

        [AllowAnonymous]
        [EnableRateLimiting("strict-login")]
        [HttpPost("Autenticar")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Autenticar(
            [FromBody] LoginRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var response = await _loginService.AutenticarAsync(request, ct);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("LoginExterno")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginExterno(
            [FromBody] ExternalLoginRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var response = await _loginService.LoginExternoAsync(request, ct);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("Cadastrar")]
        [ProducesResponseType(typeof(CadastrarLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Cadastrar(
            [FromBody] CadastrarLoginRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var response = await _loginService.CadastrarAsync(request, ct);
                // Opcional: CreatedAtAction com rota de perfil
                return Ok(response);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ======================== RESET DE SENHA ========================

        [AllowAnonymous]
        [EnableRateLimiting("strict-login")]
        [HttpPost("EsqueciSenha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EsqueciSenha(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var result = await _loginService.SolicitarResetSenhaAsync(request, ct);
                return Ok(
                    new
                    {
                        message = "Se o email existir em nossa base, você receberá um link para resetar sua senha.",
                        success = result,
                    }
                );
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [EnableRateLimiting("strict-login")]
        [HttpPost("ResetarSenha")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetarSenha(
            [FromBody] ResetPasswordRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var result = await _loginService.ResetarSenhaAsync(request, ct);
                return result
                    ? Ok(new { message = "Senha alterada com sucesso! O link de reset expirou automaticamente." })
                    : BadRequest(new { message = "Token inválido, expirado ou já foi utilizado. Solicite um novo link de reset de senha." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ======================== CONFIRMAÇÃO DE EMAIL ========================

        [AllowAnonymous]
        [HttpPost("ReenviarConfirmacao")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReenviarConfirmacao(
            [FromBody] ForgotPasswordRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var result = await _loginService.EnviarConfirmacaoEmailAsync(request.Email, ct);
                return result
                    ? Ok(new { message = "Email de confirmação enviado com sucesso!" })
                    : BadRequest(new { message = "Email não encontrado ou já confirmado." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("ConfirmarEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmarEmail(
            [FromBody] VerifyEmailRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var result = await _loginService.ConfirmarEmailAsync(request, ct);
                return result
                    ? Ok(new { message = "Email confirmado com sucesso!" })
                    : BadRequest(new { message = "Token inválido ou expirado." });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ======================== PERFIL ========================

        [Authorize]
        [HttpGet("Perfil")]
        [ProducesResponseType(typeof(PerfilResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPerfil(CancellationToken ct)
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var perfil = await _loginService.ObterPerfilAsync(loginId, ct);
                return perfil is null
                    ? NotFound(new { message = "Perfil não encontrado" })
                    : Ok(perfil);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Perfil")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AtualizarPerfil(
            [FromBody] AtualizarPerfilRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var result = await _loginService.AtualizarPerfilAsync(loginId, request, ct);
                return result
                    ? Ok(new { message = "Perfil atualizado com sucesso!" })
                    : BadRequest(
                        new
                        {
                            message = "Erro ao atualizar perfil. Verifique se os dados são válidos.",
                        }
                    );
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Nome")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AtualizarNome(
            [FromBody] AtualizarNomeRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var result = await _loginService.AtualizarNomeAsync(loginId, request, ct);
                return result
                    ? Ok(new { message = "Nome atualizado com sucesso!" })
                    : BadRequest(
                        new
                        {
                            message = "Erro ao atualizar nome. Verifique se o nome é válido e não está em uso.",
                        }
                    );
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ======================== VALIDAÇÕES ========================

        [AllowAnonymous]
        [HttpPost("ValidarUsuario")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidarUsuario(
            [FromBody] ValidarUsuarioRequest request,
            CancellationToken ct
        )
        {
            try
            {
                var usuario = request.Usuario?.Trim() ?? string.Empty;
                
                if (string.IsNullOrEmpty(usuario))
                    return BadRequest(new { success = false, message = "Nome de usuário é obrigatório" });

                var existe = await _loginService.UsuarioExisteAsync(usuario, ct);
                return Ok(new { 
                    success = true, 
                    disponivel = !existe,
                    message = existe ? "Nome de usuário já está em uso" : "Nome de usuário disponível"
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ======================== COMPATIBILIDADE ========================

        [AllowAnonymous]
        [HttpPost("GoogleAuth")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleAuth(
            [FromBody] ExternalLoginRequest request,
            CancellationToken ct
        )
        {
            request.Provider = "google";
            return await LoginExterno(request, ct);
        }

        // Endpoint de teste para email (remover em produção)
        [AllowAnonymous]
        [HttpPost("TestEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TestEmail(
            [FromBody] TestEmailRequest request,
            CancellationToken ct
        )
        {
            try
            {
                _logger.LogInformation("Teste de email solicitado para: {Email}", request.Email);
                
                var htmlBody = @"
                    <h2>Teste de Email - FastSurvey</h2>
                    <p>Este é um email de teste para verificar se o sistema de email está funcionando.</p>
                    <p>Se você recebeu este email, o sistema está configurado corretamente!</p>
                    <p>Data/Hora: " + System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</p>";

                // Injetar o serviço de email via DI
                var emailSender = HttpContext.RequestServices.GetRequiredService<IEmailSender>();
                await emailSender.SendAsync(
                    request.Email,
                    "Teste de Email - FastSurvey",
                    htmlBody,
                    ct
                );

                _logger.LogInformation("Email de teste enviado com sucesso para: {Email}", request.Email);
                return Ok(new { message = "Email de teste enviado com sucesso!" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de teste para: {Email}", request.Email);
                return BadRequest(new { message = $"Erro ao enviar email: {ex.Message}" });
            }
        }
    }
}
