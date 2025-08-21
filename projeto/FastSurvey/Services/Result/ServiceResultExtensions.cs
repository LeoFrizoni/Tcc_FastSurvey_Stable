// FASTSURVEY/Services/ServiceResultActionResultExtensions.cs
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Services.Result
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult<T>(this ServiceResult<T> result, ControllerBase controller)
        {
            if (result is null) return controller.StatusCode(500, "Erro interno.");

            if (result.Success)
            {
                return controller.Ok(result.Data);
            }

            // Handle errors (e.g., return BadRequest or other appropriate status codes)
            return controller.BadRequest(result.Errors);
        }
    }
}
