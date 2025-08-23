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
                return $"dev-token-{login.Loginid}-{Guid.NewGuid():N}";

            var issuer = cfg["Jwt:Issuer"];
            var audience = cfg["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, login.Usuario ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(ClaimTypes.NameIdentifier, login.Loginid.ToString()),
                new Claim(ClaimTypes.Name, login.Usuario ?? string.Empty),
                new Claim("loginId", login.Loginid.ToString()),
                new Claim("tipoUsuarioId", (login.Tipousuarioid ?? 10).ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

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
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        // M�todo de compatibilidade
        public static string Emitir(IConfiguration cfg, ModelLogin login)
            => GenerateToken(login, cfg);
    }
}
