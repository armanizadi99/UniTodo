using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoItemDto
    {
        /// <summary>The database identifier of this todo item.</summary>
        public int Id { get; init; }
        /// <summary>The description text of the todo item.</summary>
        public string Description { get; init; }
        /// <summary>Whether the item is completed.</summary>
        public bool IsCompleted { get; init; }
        /// <summary>When the item was completed, if applicable.</summary>
        public DateTimeOffset? CompletedAt { get; init; }
        /// <summary>The user ID of who completed the item.</summary>
        public Guid? CompletedBy { get; init; }
        /// <summary>Optional notes attached to the item.</summary>
        public string? Notes { get; init; }
        /// <summary>The user ID the item is assigned to.</summary>
        public Guid? AsignedTo { get; init; }
        /// <summary>When the item was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the item was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public TodoItemDto(int Id, string Description, bool IsCompleted, DateTimeOffset? CompletedAt, Guid? CompletedBy, string? Notes, Guid? AsignedTo, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.Description = Description;
            this.IsCompleted = IsCompleted;
            this.CompletedAt = CompletedAt;
            this.CompletedBy = CompletedBy;
            this.Notes = Notes;
            this.AsignedTo = AsignedTo;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
