namespace UniTodo.Modules.Auth.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
