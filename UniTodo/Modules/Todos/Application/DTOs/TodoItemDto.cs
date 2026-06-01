using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoItemDto(int Id, string Description, DateTimeOffset? CompletedAt, string? Notes, Guid? AsignedTo, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
