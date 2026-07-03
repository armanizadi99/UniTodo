using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
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
        private readonly TokenService _tokenService;
        private readonly AuthDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService, AuthDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
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

            var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken(user.Id);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = expiresAt,
                Email = user.Email ?? ""
            });
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshAccessTokenRequestDto dto, CancellationToken cancellationToken)
        {
            var tokenHash = _tokenService.ComputeSha256(dto.RefreshToken);
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
            if (token is null)
                return Problem(detail: "Token not found", statusCode: (int)HttpStatusCode.NotFound);

            if (token.IsRevoked)
                return Problem(detail: "Token is revoked", statusCode: (int)HttpStatusCode.BadRequest);

            if (token.ExpiresAt < DateTimeOffset.UtcNow)
                return Problem(detail: "Token is expired", statusCode: (int)HttpStatusCode.Unauthorized);

            token.IsRevoked = true;
            var newRefreshToken = _tokenService.CreateRefreshToken(token.UserId);

            var user = await _userManager.FindByIdAsync(token.UserId);
            if (user is null)
                return Problem(detail: "User not found", statusCode: (int)HttpStatusCode.NotFound);

            var (newAccessToken, expiresAt) = _tokenService.CreateAccessToken(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem(detail: "Token was already revoked by another request.",
                statusCode: (int)HttpStatusCode.Conflict);
            }

            return Ok(new RefreshAccessTokenResponseDto
            {
                RefreshToken = newRefreshToken,
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = expiresAt
            });
        }
    }
}
