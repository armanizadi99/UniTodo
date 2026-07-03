namespace UniTodo.Modules.Auth.Dtos
{
    /// <summary>The response returned after a successful login.</summary>
    public class LoginResponseDto
    {
        /// <summary>The JWT access token.</summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>The refresh token for obtaining a new access token.</summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>The UTC expiry time of the access token.</summary>
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        /// <summary>The email address of the authenticated user.</summary>
        public string Email { get; set; } = string.Empty;
    }
}
