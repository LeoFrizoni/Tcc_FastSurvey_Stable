// Services/Auth/JwtHelper.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FASTSURVEY.Services
{
    public static class JwtHelper
    {
        public static string Gerar(int loginId, int tipoUsuarioId, string usuario, IConfiguration cfg)
        {
            var issuer = cfg["Jwt:Issuer"] ?? "fastsurvey";
            var audience = cfg["Jwt:Audience"] ?? "fastsurvey.web";
            var key = cfg["Jwt:Key"] ?? "CHAVE_SUPER_SECRETA_TROCAR";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, loginId.ToString()),
                new Claim("loginId", loginId.ToString()),
                new Claim("tipoUsuarioId", tipoUsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
