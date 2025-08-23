using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FASTSURVEY.Services.Login;
using System.Security.Claims;
using System.Text.Json;

namespace FASTSURVEY.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthorizationMiddleware> _logger;

        public AuthorizationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<AuthorizationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var token = ExtractTokenFromHeader(context.Request.Headers["Authorization"]);
                
                if (!string.IsNullOrEmpty(token))
                {
                    var principal = JwtHelper.ValidateToken(token, _configuration);
                    
                    if (principal != null)
                    {
                        context.User = principal;
                        
                        // Adicionar claims customizadas se necessário
                        var loginIdClaim = principal.FindFirst("loginId");
                        var tipoUsuarioClaim = principal.FindFirst("tipoUsuarioId");
                        
                        if (loginIdClaim != null && tipoUsuarioClaim != null)
                        {
                            var claims = new List<Claim>
                            {
                                new Claim("loginId", loginIdClaim.Value),
                                new Claim("tipoUsuarioId", tipoUsuarioClaim.Value)
                            };
                            
                            context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Token inválido: {Token}", token.Substring(0, Math.Min(10, token.Length)) + "...");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar autorização");
            }

            await _next(context);
        }

        private static string? ExtractTokenFromHeader(string? authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            return authorizationHeader.Substring("Bearer ".Length).Trim();
        }
    }

    public static class AuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}
