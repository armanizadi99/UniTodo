using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListTemplateDto
    {
        /// <summary>The database identifier of this template.</summary>
        public int Id { get; init; }
        /// <summary>The display name of the template.</summary>
        public string Name { get; init; }
        /// <summary>The reset policy that new runs inherit from this template.</summary>
        public ResetPolicy ResetPolicy { get; init; }
        /// <summary>The current status of the template.</summary>
        public TodoListStatus Status { get; init; }
        /// <summary>When the template was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the template was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public TodoListTemplateDto(int Id, string Name, ResetPolicy ResetPolicy, TodoListStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.Name = Name;
            this.ResetPolicy = ResetPolicy;
            this.Status = Status;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
