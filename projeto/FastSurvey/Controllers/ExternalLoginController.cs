using FASTSURVEY.Dtos.Login;
using FASTSURVEY.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly IExternalLoginService _svc;
        public ExternalLoginController(IExternalLoginService svc) => _svc = svc;

        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] ExternalLoginRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var resp = await _svc.GoogleAsync(req, ct);
            return Ok(resp);
        }
    }

}
