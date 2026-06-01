using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class TodoListRunMappingExtensions
    {
        public static TodoListRunDto ToDto(this TodoListRun run)
        {
            return new TodoListRunDto(
                run.Id,
                run.RunId,
    run.Name,
    run.ResetPolicy,
    run.ownerId.Value,
    run.Status,
    run.IsShared,
    run.ClosedAt,
    run.CreatedAt,
    run.UpdatedAt);
        }
    }
}
