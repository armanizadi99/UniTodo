using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class TodoItemTemplateMappingExtensions
    {
        public static TodoItemTemplateDto ToDto(this TodoItemTemplate template)
        {
            return new TodoItemTemplateDto(
                template.Id,
                template.Description.Value,
                template.CreatedAt,
                template.UpdatedAt);
        }
    }
}