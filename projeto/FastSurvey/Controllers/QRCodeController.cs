using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.QRCode;
using FASTSURVEY.Services.Result; // ToActionResult
using FASTSURVEY.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrService;
        private readonly FASTSURVEY.Services.Security.IAuthorizationService _authService;

        public QRCodeController(
            IQRCodeService qrService,
            FASTSURVEY.Services.Security.IAuthorizationService authService
        )
        {
            _qrService = qrService;
            _authService = authService;
        }

        /// <summary>Gera (ou regenera) um QR Code para a pesquisa.</summary>
        [HttpPost("gerar", Name = "GerarQRCode")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(QRCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GerarQRCode(
            [FromBody] QRCodeRequest request,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _qrService.GerarQRCodeAsync(request, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Obtém o QR Code (gera se não existir) para a pesquisa.</summary>
        [HttpGet("pesquisa/{pesquisaId:int}", Name = "ObterQRCodePesquisa")]
        [ProducesResponseType(typeof(QRCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterQRCodePesquisa(
            [FromRoute] int pesquisaId,
            CancellationToken ct = default
        )
        {
            var result = await _qrService.ObterQRCodePesquisaAsync(pesquisaId, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Valida a URL do QR Code (endpoint público).</summary>
        [HttpGet("validar", Name = "ValidarQRCode")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidarQRCode(
            [FromQuery] string url,
            CancellationToken ct = default
        )
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest(new { message = "URL é obrigatória." });

            var result = await _qrService.ValidarQRCodeAsync(url, ct);
            // Padroniza payload de sucesso como { valido = true/false }
            if (result.Success)
                return Ok(new { valido = result.Data });
            return result.ToActionResult(this);
        }

        /// <summary>Atualiza configurações/expiração e (opcionalmente) regenera o QR Code.</summary>
        [HttpPut("pesquisa/{pesquisaId:int}", Name = "AtualizarQRCode")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(QRCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AtualizarQRCode(
            [FromRoute] int pesquisaId,
            [FromBody] QRCodeRequest request,
            CancellationToken ct = default
        )
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            request.PesquisaId = pesquisaId;
            var result = await _qrService.AtualizarQRCodeAsync(pesquisaId, request, ct);
            return result.ToActionResult(this);
        }

        /// <summary>Lista todos os QR Codes do usuário (por loginId).</summary>
        [HttpGet("usuario/{loginId:int}", Name = "ListarQRCodesUsuario")]
        [ProducesResponseType(typeof(List<QRCodeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListarQRCodesUsuario(
            [FromRoute] int loginId,
            CancellationToken ct = default
        )
        {
            // Autorização básica: o próprio usuário ou admin
            var currentUserId = _authService.GetCurrentUserId(User);
            var currentUserType = _authService.GetCurrentUserType(User);

            if (currentUserId != loginId && !_authService.IsAdmin(currentUserType))
                return Forbid();

            var result = await _qrService.ListarQRCodesUsuarioAsync(loginId, ct);
            return result.ToActionResult(this);
        }
    }
}
