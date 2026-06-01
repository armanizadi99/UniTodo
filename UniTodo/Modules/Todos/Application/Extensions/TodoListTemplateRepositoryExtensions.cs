using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    internal static class TodoListTemplateRepositoryExtensions
    {
        internal static async Task<TodoListTemplate> GetTodoListTemplateByIdOrThrowAsync(this ITodoListTemplateRepository repository, int id, bool includesTodoItemTemplates = false, CancellationToken cancellationToken = default)
        {
            var todoListTemplate = await repository.GetTodoListTemplateByIdAsync(id, includesTodoItemTemplates, cancellationToken);
            if (todoListTemplate is null)
                throw new DomainEntityNotFoundException(nameof(TodoListTemplate), id);
            return todoListTemplate;
        }
    }
}
