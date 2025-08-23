using FASTSURVEY.Dtos.Login.Avatar;
using FASTSURVEY.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvatarController : ControllerBase
    {
        private readonly IAvatarService _svc;
        public AvatarController(IAvatarService svc) => _svc = svc;

        [Authorize]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] AvatarUploadRequest req, CancellationToken ct)
        {
            var loginId = int.Parse(User.FindFirst("loginId")!.Value);
            var resp = await _svc.UploadAsync(loginId, req, ct);
            return Ok(resp);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(CancellationToken ct)
        {
            var loginId = int.Parse(User.FindFirst("loginId")!.Value);
            await _svc.RemoverAsync(loginId, ct);
            return NoContent();
        }
    }
}
