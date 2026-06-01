using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListRunDto(int Id, Guid RunId, string Name, ResetPolicy ResetPolicy, Guid OwnerId, TodoListRunStatus Status, bool IsShared, DateTimeOffset? ClosedAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
