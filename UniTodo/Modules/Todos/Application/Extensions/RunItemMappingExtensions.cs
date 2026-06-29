using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RunItemMappingExtensions
    {
        public static RunItemDto ToRunItemDto(this RunItem item)
        {
            return new RunItemDto(item.Id, item.Description.Value, item.IsCompleted, item.CompletedAt, item.CompletedBy?.Value, item.Notes?.Value, item.AssignedTo?.Value, item.CreatedAt, item.UpdatedAt);
        }
    }
}
