using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniTodo.Modules.Auth.DB;
using UniTodo.Modules.Auth.Dtos;

namespace UniTodo.Modules.Auth.Controllers
{
[ApiController]
[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

public AuthController( UserManager<ApplicationUser> userManager )
        {
        _userManager = userManager;
        }

[HttpPost("register")]
public async Task<IActionResult> RegisterAsync([FromBody]RegisterRequestDto dto)
{
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

var result = await _userManager.CreateAsync(user, dto.Password);
if(!result.Succeeded)
{
        var errors = result.Errors.Select(e => e.Description);
        return BadRequest(new {Errors = errors});
        }

        return Ok(new
        {
            Id = user.Id,
Email = user.Email
        });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync( [FromBody] LoginRequestDto dto )
        {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
        return Unauthorized(new
        {
            Error = "Username or password is invalid"
        });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
        return Unauthorized(new
        {
            Error = "Username or password is invalid"
        });
        }

        var token = CreateJwtToken(user);

        return Ok(new
        {
Token = token,
Email = user.Email
        });
        }

private string CreateJwtToken(ApplicationUser user)
{
        var secretKey = Encoding.UTF8.GetBytes("thisisjut a fucking temp passwword I'll change it later, I just need to test something right now. Hardcoding such a code in my codebase might not be a big security issue because this code is running on a server trusted server, but well, because other parts of the applications might require it or well gonna right the rest.");
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
            Expires = DateTime.UtcNow.AddHours(2),
SigningCredentials = credentials,
Issuer = "my own app",
Audience = "for my todo app"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
        }
    }
}
