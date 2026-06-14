using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniTodo.Modules.Auth.DB;
using UniTodo.Modules.Auth.Dtos;
using UniTodo.Modules.Auth.Services;

namespace UniTodo.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenCreater _tokenCreater;

        public AuthController(UserManager<ApplicationUser> userManager, JwtTokenCreater tokenCreater)
        {
            _userManager = userManager;
_tokenCreater = tokenCreater;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto dto)
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

            var token = _tokenCreater.CreateJwtToken(user);

            return Ok(new
            {
                Token = token,
                Email = user.Email
            });
        }
    }
}
