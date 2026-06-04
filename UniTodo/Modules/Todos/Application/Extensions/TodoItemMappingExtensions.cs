using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Application.DTOs;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class TodoItemMappingExtensions
    {
        public static TodoItemDto ToTodoItemDto(this TodoItem item)
        {
            return new TodoItemDto(item.Id, item.Description.Value, item.IsCompleted, item.CompletedAt, item.CompletedBy?.Value, item.Notes?.Value, item.AsignedTo?.Value, item.CreatedAt, item.UpdatedAt);
        }
    }
}
