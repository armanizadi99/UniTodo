namespace UniTodo.Modules.Auth.Dtos
{
    public class RefreshAccessTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
    }
}
