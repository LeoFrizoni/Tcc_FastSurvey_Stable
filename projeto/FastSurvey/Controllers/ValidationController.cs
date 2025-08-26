#nullable enable
using FASTSURVEY.Services.Mobile;
using FASTSURVEY.Services.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ValidationController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IMobileService _mobileService;
        private readonly ValidationOptions _options;

        public ValidationController(
            IValidationService validationService,
            IMobileService mobileService,
            IOptions<ValidationOptions> options
        )
        {
            _validationService = validationService;
            _mobileService = mobileService;
            _options = options.Value;
        }

        [HttpPost("pesquisa")]
        public IActionResult ValidatePesquisa(
            [FromBody] FASTSURVEY.Dtos.Pesquisas.CriarPesquisaRequest request
        )
        {
            var vr = _validationService.ValidatePesquisa(request);
            if (!vr.Success)
                return BadRequest(
                    new
                    {
                        success = false,
                        errors = vr.Errors,
                        deviceInfo = GetDeviceInfoInternal(),
                    }
                );

            return Ok(
                new
                {
                    success = true,
                    message = "Pesquisa válida",
                    deviceInfo = GetDeviceInfoInternal(),
                }
            );
        }

        [HttpPost("pergunta")]
        public IActionResult ValidatePergunta(
            [FromBody] FASTSURVEY.Dtos.Perguntas.CriarPerguntaRequest request
        )
        {
            var vr = _validationService.ValidatePergunta(request);
            if (!vr.Success)
                return BadRequest(
                    new
                    {
                        success = false,
                        errors = vr.Errors,
                        deviceInfo = GetDeviceInfoInternal(),
                    }
                );

            return Ok(
                new
                {
                    success = true,
                    message = "Pergunta válida",
                    deviceInfo = GetDeviceInfoInternal(),
                }
            );
        }

        [HttpPost("resposta")]
        public IActionResult ValidateResposta(
            [FromBody] FASTSURVEY.Dtos.Respostas.CriarRespostaDiscursivaRequest request
        )
        {
            var vr = _validationService.ValidateResposta(request);
            if (!vr.Success)
                return BadRequest(
                    new
                    {
                        success = false,
                        errors = vr.Errors,
                        deviceInfo = GetDeviceInfoInternal(),
                    }
                );

            return Ok(
                new
                {
                    success = true,
                    message = "Resposta válida",
                    deviceInfo = GetDeviceInfoInternal(),
                }
            );
        }

        // Upload precisa vir como multipart/form-data
        [HttpPost("file")]
        [RequestSizeLimit(long.MaxValue)]
        public IActionResult ValidateFile([FromForm] IFormFile file)
        {
            var allowed =
                _options.AllowedFileExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
            var maxBytes = (long)_options.MaxFileSizeInMB * 1024 * 1024;

            var vr = _validationService.ValidateFileUpload(file, allowed, maxBytes);
            if (!vr.Success)
                return BadRequest(
                    new
                    {
                        success = false,
                        errors = vr.Errors,
                        deviceInfo = GetDeviceInfoInternal(),
                    }
                );

            return Ok(
                new
                {
                    success = true,
                    message = "Arquivo válido",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    deviceInfo = GetDeviceInfoInternal(),
                }
            );
        }

        [HttpGet("device-info")]
        [AllowAnonymous]
        public IActionResult GetDeviceInfo()
        {
            var info = GetDeviceInfoInternal();
            return Ok(info);
        }

        [HttpPost("email")]
        [AllowAnonymous]
        public IActionResult ValidateEmail([FromBody] EmailValidationRequest request)
        {
            var vr = _validationService.ValidateEmail(request.Email);
            if (!vr.Success)
                return BadRequest(new { success = false, errors = vr.Errors });
            return Ok(new { success = true, message = "Email válido" });
        }

        [HttpPost("password")]
        [AllowAnonymous]
        public IActionResult ValidatePassword([FromBody] PasswordValidationRequest request)
        {
            var vr = _validationService.ValidatePassword(request.Password);
            if (!vr.Success)
                return BadRequest(new { success = false, errors = vr.Errors });
            return Ok(new { success = true, message = "Senha válida" });
        }

        [HttpPost("username")]
        [AllowAnonymous]
        public IActionResult ValidateUsername([FromBody] UsernameValidationRequest request)
        {
            var vr = _validationService.ValidateUsername(request.Username);
            if (!vr.Success)
                return BadRequest(new { success = false, errors = vr.Errors });
            return Ok(new { success = true, message = "Nome de usuário válido" });
        }

        // ----- helpers -----
        private object GetDeviceInfoInternal()
        {
            var deviceInfo = _mobileService.GetDeviceInfo(HttpContext);
            var optimization = _mobileService.GetOptimizationOptions(HttpContext);

            return new
            {
                deviceInfo = new
                {
                    deviceType = deviceInfo.DeviceType,
                    operatingSystem = deviceInfo.OperatingSystem,
                    browser = deviceInfo.Browser,
                    isTouchCapable = deviceInfo.IsTouchCapable,
                    isMobile = deviceInfo.IsMobile,
                    isTablet = deviceInfo.IsTablet,
                },
                optimizationOptions = new
                {
                    useTouchOptimizedButtons = optimization.UseTouchOptimizedButtons,
                    enableSwipeGestures = optimization.EnableSwipeGestures,
                    useSimplifiedLayout = optimization.UseSimplifiedLayout,
                    touchTargetSize = optimization.TouchTargetSize,
                    enableHapticFeedback = optimization.EnableHapticFeedback,
                    optimizeImages = optimization.OptimizeImages,
                    useMobileNavigation = optimization.UseMobileNavigation,
                },
            };
        }
    }

    public class EmailValidationRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordValidationRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class UsernameValidationRequest
    {
        public string Username { get; set; } = string.Empty;
    }
}
