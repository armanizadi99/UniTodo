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
        /// <remarks>
        /// Creates a new user account with the provided email and password.
        /// A user must register before they can authenticate and access protected resources.
        ///
        /// The password must meet ASP.NET Identity's default strength requirements
        /// (minimum length, at least one non-alphanumeric character, at least one digit,
        /// at least one uppercase letter).
        ///
        /// Returns 400 Bad Request if the email is already taken or the password
        /// does not meet the strength requirements. The error detail message will
        /// describe the specific validation failures.
        /// </remarks>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
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

            return Ok(new RegisterResponseDto(user.Id, user.Email ?? ""));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT access token and refresh token.
        /// </summary>
        /// <param name="dto">The login credentials including email and password.</param>
        /// <returns>An access token, refresh token, access token expiration time, and the user's email.</returns>
        /// <remarks>
        /// Authenticates a user by email and password. On success, returns a
        /// short-lived JWT access token and a long-lived refresh token.
        ///
        /// The access token must be included in the Authorization header as a Bearer token
        /// for all subsequent authenticated requests. When the access token expires, use
        /// the refresh endpoint to obtain a new pair without re-entering credentials.
        ///
        /// Keep the refresh token confidential — if compromised, an attacker can
        /// generate new access tokens until the refresh token expires or is revoked.
        ///
        /// Returns 401 Unauthorized if the email or password is incorrect.
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Exchanges a valid refresh token for a new access token and refresh token.
        /// </summary>
        /// <param name="dto">The refresh token to exchange.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A new access token and refresh token pair with expiration details.</returns>
        /// <remarks>
        /// Exchanges a valid, non-revoked, non-expired refresh token for a fresh
        /// access token and refresh token pair. The submitted refresh token is
        /// immediately revoked and can no longer be used.
        ///
        /// This allows the client to maintain an authenticated session without
        /// requiring the user to re-enter their password. Store the new refresh token
        /// and discard the old one.
        ///
        /// Error responses:
        /// - 400 Bad Request: The refresh token has already been revoked.
        /// - 401 Unauthorized: The refresh token has expired.
        /// - 404 Not Found: The refresh token does not exist.
        /// - 409 Conflict: A concurrent request already exchanged this token.
        /// </remarks>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshAccessTokenResponseDto), StatusCodes.Status200OK)]
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