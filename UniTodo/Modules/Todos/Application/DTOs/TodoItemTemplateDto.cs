namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoItemTemplateDto
    {
        /// <summary>The database identifier of this template item.</summary>
        public int Id { get; init; }
        /// <summary>The description text of the template item.</summary>
        public string Description { get; init; }
        /// <summary>When the template item was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the template item was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public TodoItemTemplateDto(int Id, string Description, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.Description = Description;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
