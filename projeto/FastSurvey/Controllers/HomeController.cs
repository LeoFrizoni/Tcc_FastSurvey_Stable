using FASTSURVEY.Services.Mobile;
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
{
    private readonly IMobileService _mobile;

    public HomeController(IMobileService mobile) => _mobile = mobile;

    [HttpGet]
    public IActionResult Index()
    {
        var info = _mobile.GetDeviceInfo(HttpContext);
        var options = _mobile.GetOptimizationOptions(HttpContext);
        var useMobileLayout = _mobile.ShouldUseMobileLayout(HttpContext);

        return Ok(new
        {
            deviceInfo = info,
            optimizationOptions = options,
            useMobileLayout = useMobileLayout
        });
    }
}
