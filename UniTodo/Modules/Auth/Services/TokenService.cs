using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniTodo.Modules.Auth.DB;
using UniTodo.Modules.Auth.Models;

namespace UniTodo.Modules.Auth.Services
{
    public class TokenService
    {
        private readonly JwtSettings _settings;
        private readonly AuthDbContext _context;

        public TokenService(JwtSettings settings, AuthDbContext context)
        {
            _settings = settings;
            _context = context;
        }

        public (string, DateTime) CreateAccessToken(ApplicationUser user)
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

            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = credentials,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expiresAt);
        }

        public string CreateRefreshToken(string userId)
        {
            string randomToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var token = new RefreshToken
            {
                UserId = userId,
                TokenHash = ComputeSha256(randomToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(token);
            return randomToken;
        }

        public string ComputeSha256(string data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
        }
    }
}
