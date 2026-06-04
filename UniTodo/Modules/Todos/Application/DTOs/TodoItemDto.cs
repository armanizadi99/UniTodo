using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoItemDto(int Id, string Description, bool IsCompleted, DateTimeOffset? CompletedAt, Guid? CompletedBy, string? Notes, Guid? AsignedTo, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
