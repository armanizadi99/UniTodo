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
    /// <summary>
    /// Controller for user authentication, including registration and login.
    /// </summary>
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

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="dto">The registration details including email and password.</param>
        /// <returns>The newly created user's identifier and email.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
                return Problem(
                    detail: string.Join("; ", errors),
                    title: "Registration Failed",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return Ok(new
            {
                Id = user.Id,
                Email = user.Email
            });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="dto">The login credentials including email and password.</param>
        /// <returns>A JWT token and the user's email address.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return Problem(
                    detail: "Username or password is invalid",
                    title: "Authentication Failed",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
            {
                return Problem(
                    detail: "Username or password is invalid",
                    title: "Authentication Failed",
                    statusCode: StatusCodes.Status401Unauthorized);
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
