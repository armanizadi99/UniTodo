using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunIterationMappingExtensions
    {
        public static RunIterationDto ToRunIterationDto(this RunIteration iteration)
        {
            return new RunIterationDto(
                iteration.Id,
                iteration.ClosedAt,
                iteration.RunItems.Select(i => i.ToRunItemDto()).ToList(),
                iteration.CreatedAt,
                iteration.UpdatedAt);
        }
    }
}
