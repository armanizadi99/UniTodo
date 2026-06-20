namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListRunMemberDto
    {
        /// <summary>The database identifier of this membership record.</summary>
        public int Id { get; init; }
        /// <summary>The user ID of the member.</summary>
        public Guid UserId { get; init; }
        /// <summary>When the member was added to the run.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the membership was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public TodoListRunMemberDto(int Id, Guid UserId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.UserId = UserId;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
