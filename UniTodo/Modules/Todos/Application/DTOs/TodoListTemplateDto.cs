using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListTemplateDto(int Id, string name, ResetPolicy ResetPolicy, TodoListStatus Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
}
