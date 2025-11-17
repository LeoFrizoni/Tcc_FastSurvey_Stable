using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FASTSURVEY.Middleware
{
    /// <summary>
    /// Complementa claims que possam faltar (ex.: loginId, tipoUsuarioId).
    /// NÃO revalida JWT (isso já é feito pelo JwtBearer).
    /// </summary>
    public sealed class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(
            RequestDelegate next,
            ILogger<AuthorizationMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var user = context.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    // Se já autenticado, só garante que as claims custom existem
                    var identity =
                        user.Identities.FirstOrDefault(i => i.IsAuthenticated)
                        ?? user.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        // NÃO remova claims existentes; apenas adicione se faltar
                        if (
                            !identity.HasClaim(c => c.Type == "loginId")
                            && user.FindFirst("loginId") is { } loginId
                        )
                            identity.AddClaim(loginId);

                        if (
                            !identity.HasClaim(c => c.Type == "tipoUsuarioId")
                            && user.FindFirst("tipoUsuarioId") is { } tipoUsuarioId
                        )
                            identity.AddClaim(tipoUsuarioId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao complementar claims");
                // não bloquear request
            }

            await _next(context);
        }
    }

    public static class AuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationMiddleware(
            this IApplicationBuilder app
        ) => app.UseMiddleware<AuthorizationMiddleware>();
    }
}
