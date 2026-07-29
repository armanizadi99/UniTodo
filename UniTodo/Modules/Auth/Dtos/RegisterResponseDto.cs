namespace UniTodo.Modules.Auth.Dtos
{
    public record RegisterResponseDto
    {
        public string Id { get; init; }
        public string Email { get; init; }

        public RegisterResponseDto(string Id, string Email)
        {
            this.Id = Id;
            this.Email = Email;
        }
    }
}