#nullable enable
using System.Linq;
using Microsoft.AspNetCore.Http; // StatusCodes
using Microsoft.AspNetCore.Mvc;

namespace FASTSURVEY.Services.Result
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult<T>(
            this ServiceResult<T> result,
            ControllerBase controller
        )
        {
            if (result is null)
                return controller.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { code = "NULL_RESULT", message = "Erro interno." }
                );

            if (result.Success)
                return controller.Ok(result.Data);

            var code = (result.Code ?? string.Empty).Trim().ToUpperInvariant();

            var payload = new
            {
                success = false,
                code = code,
                message = result.Message,
                errors = result.Errors, // List<string>?
            };

            return code switch
            {
                "NOT_FOUND" => controller.NotFound(payload),
                "UNAUTHORIZED" => controller.Unauthorized(payload),
                "FORBIDDEN" => controller.StatusCode(StatusCodes.Status403Forbidden, payload),
                "VALIDATION_ERROR" => controller.BadRequest(payload),
                "BAD_REQUEST" => controller.BadRequest(payload),
                // se você usar "CONFLICT", "GONE", etc., adicione aqui
                _ => controller.StatusCode(StatusCodes.Status500InternalServerError, payload),
            };
        }

        // Helpers compatíveis com List<string>
        public static string? FirstError<T>(this ServiceResult<T> result) =>
            result.Errors?.FirstOrDefault();

        public static bool HasErrors<T>(this ServiceResult<T> result) =>
            result.Errors is { Count: > 0 };
    }
}
