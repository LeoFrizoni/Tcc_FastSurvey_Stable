using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class ServicesToken
{
    private readonly string _secretKey;
    private readonly int _expirationTime;

    public ServicesToken(string secretKey, int expirationTime = 60)
    {
        _secretKey = secretKey;
        _expirationTime = expirationTime;
    }

    public string GenerateToken(string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userId),
            // Você pode adicionar mais claims, como roles, permissões, etc.
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "FastSurveyApp",
            audience: "FastSurveyApp",
            claims: claims,
            expires: DateTime.Now.AddMinutes(_expirationTime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "FastSurveyApp",
                ValidAudience = "FastSurveyApp",
                IssuerSigningKey = key
            }, out var validatedToken);

            return principal;
        }
        catch
        {
            return null; // Token inválido
        }
    }
}
