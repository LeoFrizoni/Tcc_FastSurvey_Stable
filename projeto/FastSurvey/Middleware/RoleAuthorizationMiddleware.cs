using FASTSURVEY.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Middleware
{
    public sealed class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RoleAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<RoleAuthorizationMiddleware> logger,
            IServiceScopeFactory scopeFactory
        )
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path;

                if (!RequiresSpecialAuthorization(path))
                {
                    await _next(context);
                    return;
                }

                var user = context.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning(
                        "Acesso não autenticado: {Method} {Path}",
                        context.Request.Method,
                        path
                    );
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

                // Admin area
                if (IsAdminArea(path))
                {
                    var userType = authService.GetCurrentUserType(user);
                    if (!authService.IsAdmin(userType))
                    {
                        _logger.LogWarning(
                            "Admin negado. UserId={UserId} Type={UserType} {Method} {Path}",
                            authService.GetCurrentUserId(user),
                            userType,
                            context.Request.Method,
                            path
                        );
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden - Admin access required");
                        return;
                    }
                }

                // Premium area
                if (IsPremiumResource(path))
                {
                    var userType = authService.GetCurrentUserType(user);
                    if (!authService.CanAccessPremiumFeatures(userType))
                    {
                        _logger.LogWarning(
                            "Premium negado. UserId={UserId} Type={UserType} {Method} {Path}",
                            authService.GetCurrentUserId(user),
                            userType,
                            context.Request.Method,
                            path
                        );
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden - Premium access required");
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no RoleAuthorizationMiddleware");
                // deixa seguir; controllers ainda podem validar
                await _next(context);
            }
        }

        // Só autorizações especiais (Swagger, static, login etc. ficam fora)
        private static bool RequiresSpecialAuthorization(PathString path)
        {
            var p = path.Value?.ToLowerInvariant() ?? string.Empty;

            // públicos
            if (p.StartsWith("/swagger") || p.StartsWith("/health") || p.StartsWith("/error"))
                return false;

            if (
                p.Contains("/login")
                || p.Contains("/cadastrar")
                || p.Contains("/esqueci-senha")
                || p.Contains("/resetar-senha")
                || p.Contains("/confirmar-email")
                || p.Contains("/google")
                || p.Contains("/test")
            )
                return false;

            // premium/admin/export/resultados/interativa
            return IsAdminArea(path)
                || IsPremiumResource(path)
                || p.StartsWith("/export")
                || p.StartsWith("/resultados");
        }

        private static bool IsAdminArea(PathString path) =>
            path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase);

        private static bool IsPremiumResource(PathString path)
        {
            // Segmente início para reduzir falso-positivo
            return path.StartsWithSegments(
                    "/pesquisa-interativa",
                    StringComparison.OrdinalIgnoreCase
                ) || path.StartsWithSegments("/graficos", StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class RoleAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder app) =>
            app.UseMiddleware<RoleAuthorizationMiddleware>();
    }
}
