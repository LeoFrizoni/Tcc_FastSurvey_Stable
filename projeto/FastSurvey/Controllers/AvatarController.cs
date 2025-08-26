#nullable enable
using FASTSURVEY.Dtos.Login.Avatar;
using FASTSURVEY.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvatarController : BaseController
    {
        private readonly IAvatarService _service;

        public AvatarController(IAvatarService service) => _service = service;

        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10MB (ajuste conforme MaxSizeBytes)
        [ProducesResponseType(typeof(AvatarResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(
            [FromForm] AvatarUploadRequest req,
            CancellationToken ct
        )
        {
            try
            {
                var loginId = GetLoginIdFromToken();
                var resp = await _service.UploadAsync(loginId, req, ct);
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Remover(CancellationToken ct)
        {
            var loginId = GetLoginIdFromToken();
            await _service.RemoverAsync(loginId, ct);
            return Ok(new { success = true });
        }
    }
}
