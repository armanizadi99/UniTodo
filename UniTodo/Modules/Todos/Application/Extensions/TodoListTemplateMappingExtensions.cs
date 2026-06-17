using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class TodoListTemplateMappingExtensions
    {
        public static TodoListTemplateDto ToDto(this TodoListTemplate template)
        {
            return new TodoListTemplateDto(
                template.Id,
                template.Name,
                template.ResetPolicy,
                template.Status,
                template.CreatedAt,
                template.UpdatedAt);
        }
    }
}