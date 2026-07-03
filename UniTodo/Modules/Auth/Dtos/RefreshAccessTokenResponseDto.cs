namespace UniTodo.Modules.Auth.Dtos
{
    /// <summary>The response returned after successfully refreshing an access token.</summary>
    public class RefreshAccessTokenResponseDto
    {
        /// <summary>The new JWT access token.</summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>The new refresh token (the previous one is revoked).</summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>The UTC expiry time of the new access token.</summary>
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
    }
}
