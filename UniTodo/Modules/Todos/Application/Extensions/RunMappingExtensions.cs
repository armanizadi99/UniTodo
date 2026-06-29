using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunMappingExtensions
    {
        public static RunDto ToDto(this Run run)
        {
            return new RunDto(
                run.Id,
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
