using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface ITodoListRunRepository : IRepository<TodoListRun>
    {
        Task<TodoListRun?> GetTodoListRunByIdAsync(int id, bool includeItems = false, CancellationToken cancellationToken = default);
        Task<TodoListRun?> GetTodoListRunByIdAsync(int id, int itemId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoListRun>> GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
