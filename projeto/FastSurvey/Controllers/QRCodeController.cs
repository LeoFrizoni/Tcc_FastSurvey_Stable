using FASTSURVEY.Dtos.Pesquisas;
using FASTSURVEY.Services.QRCode;
using FASTSURVEY.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QRCodeController : ControllerBase
    {
        private readonly IQRCodeService _qrService;
        private readonly FASTSURVEY.Services.Security.IAuthorizationService _authService;

        public QRCodeController(IQRCodeService qrService, FASTSURVEY.Services.Security.IAuthorizationService authService)
        {
            _qrService = qrService;
            _authService = authService;
        }

        // POST: api/qrcode/gerar
        [HttpPost("gerar")]
        public async Task<IActionResult> GerarQRCode(
            [FromBody] QRCodeRequest request,
            CancellationToken ct)
        {
            var result = await _qrService.GerarQRCodeAsync(request, ct);
            return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
        }

        // GET: api/qrcode/pesquisa/{pesquisaId}
        [HttpGet("pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> ObterQRCodePesquisa(
            [FromRoute] int pesquisaId,
            CancellationToken ct)
        {
            var result = await _qrService.ObterQRCodePesquisaAsync(pesquisaId, ct);
            return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
        }

        // GET: api/qrcode/validar
        [HttpGet("validar")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidarQRCode(
            [FromQuery] string url,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL é obrigatória");

            var result = await _qrService.ValidarQRCodeAsync(url, ct);
            return result.Success ? Ok(new { valido = result.Data }) : BadRequest(result.Errors);
        }

        // PUT: api/qrcode/pesquisa/{pesquisaId}
        [HttpPut("pesquisa/{pesquisaId:int}")]
        public async Task<IActionResult> AtualizarQRCode(
            [FromRoute] int pesquisaId,
            [FromBody] QRCodeRequest request,
            CancellationToken ct)
        {
            request.PesquisaId = pesquisaId;
            var result = await _qrService.AtualizarQRCodeAsync(pesquisaId, request, ct);
            return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
        }

        // GET: api/qrcode/usuario/{loginId}
        [HttpGet("usuario/{loginId:int}")]
        public async Task<IActionResult> ListarQRCodesUsuario(
            [FromRoute] int loginId,
            CancellationToken ct)
        {
            // Verificar se o usuário pode acessar os QR Codes deste login
            var currentUserId = _authService.GetCurrentUserId(User);
            var currentUserType = _authService.GetCurrentUserType(User);

            if (currentUserId != loginId && !_authService.IsAdmin(currentUserType))
                return Forbid("Acesso negado");

            var result = await _qrService.ListarQRCodesUsuarioAsync(loginId, ct);
            return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
        }
    }
}
