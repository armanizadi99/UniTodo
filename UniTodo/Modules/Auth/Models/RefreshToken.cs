namespace UniTodo.Modules.Auth.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool IsRevoked { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
