using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniTodo.Modules.Auth.DB;

namespace UniTodo.Modules.Auth.Services
{
    public class JwtTokenCreater
    {
        private readonly JwtSettings _settings;

public JwtTokenCreater(JwtSettings settings)
{
_settings = settings;
        }

        public string CreateJwtToken( ApplicationUser user )
        {
        var secretKey = Encoding.UTF8.GetBytes(_settings.SecretSigningKey);
        var claims = new[]
        {
new Claim(JwtRegisteredClaimNames.Sub, user.Id),
new Claim(JwtRegisteredClaimNames.Email, user?.Email ?? ""),
new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
        }
    }
}
