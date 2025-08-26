#nullable enable
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
// alias do model
using ModelLogin = SISTEMA_FASTSURVEY.MODEL.Models.Login;

namespace FASTSURVEY.Services.Login
{
    internal static class JwtHelper
    {
        public static string GenerateToken(ModelLogin login, IConfiguration cfg)
        {
            var key = cfg["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
                key = "FastSurveySecretKey2024!@#$%^&*()_+"; // fallback dev only

            var issuer = cfg["Jwt:Issuer"] ?? "FastSurvey";
            var audience = cfg["Jwt:Audience"] ?? "FastSurveyUsers";

            // expiração configurável (default 24h)
            var expHoursStr = cfg["Jwt:ExpirationHours"];
            var expHours = 24;
            _ = int.TryParse(expHoursStr, out expHours);
            if (expHours <= 0)
                expHours = 24;

            var now = DateTime.UtcNow;
            var expires = now.AddHours(expHours);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tipoId = login.TipoUsuarioId ?? 13; // 13=User, 14=Premium, 15=Admin
            var role = tipoId switch
            {
                15 => "Admin",
                14 => "Premium",
                _ => "User",
            };

            // ================================================
            // Claims necessárias para o sistema
            // ================================================
            var claims = new[]
            {
                // JWT padrão
                new Claim(JwtRegisteredClaimNames.Sub, login.Usuario ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    EpochTime.GetIntDate(now).ToString(),
                    ClaimValueTypes.Integer64
                ),
                new Claim(
                    JwtRegisteredClaimNames.Nbf,
                    EpochTime.GetIntDate(now).ToString(),
                    ClaimValueTypes.Integer64
                ),
                // Padrão Microsoft
                new Claim(ClaimTypes.NameIdentifier, login.LoginId.ToString()),
                new Claim(ClaimTypes.Name, login.Usuario ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
                // Custom esperado pela BaseController
                new Claim("LoginId", login.LoginId.ToString()), // int
                new Claim("TipoUsuarioId", tipoId.ToString()), // int
                new Claim("email", login.Email ?? string.Empty), // string
                new Claim("nome", login.Usuario ?? string.Empty), // string
                // Custom adicional (retrocompatibilidade)
                new Claim("role", role),
                new Claim("loginId", login.LoginId.ToString()), // minúsculo para quem usava antes
                new Claim("tipoUsuarioId", tipoId.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida o JWT manualmente (útil em cenários fora do pipeline).
        /// </summary>
        public static ClaimsPrincipal? ValidateToken(string token, IConfiguration cfg)
        {
            try
            {
                var key = cfg["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(key))
                    return null;

                var issuer = cfg["Jwt:Issuer"];
                var audience = cfg["Jwt:Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();
                var keyBytes = Encoding.UTF8.GetBytes(key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = !string.IsNullOrEmpty(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrEmpty(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role,
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        // Atalho
        public static string Emitir(IConfiguration cfg, ModelLogin login) =>
            GenerateToken(login, cfg);
    }
}
