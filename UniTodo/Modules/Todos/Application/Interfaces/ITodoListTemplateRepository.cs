using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface ITodoListTemplateRepository : IRepository<TodoListTemplate>
    {
        Task<bool> IsNameDuplicateAsync(string name, CancellationToken cancellationToken = default);
        Task<List<TodoListTemplate>> GetUserTodoListTemplatesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<TodoListTemplate?> GetTodoListTemplateByIdAsync(int id, bool includeTodoItemTemplates = false, CancellationToken cancellationToken = default);
    }
}