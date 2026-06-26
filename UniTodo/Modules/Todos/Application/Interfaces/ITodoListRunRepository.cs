using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface ITodoListRunRepository : IRepository<TodoListRun>
    {
        Task<TodoListRun?> GetTodoListRunByRunIdAsync(Guid runId, bool includeItems = false, CancellationToken cancellationToken = default);
        Task<TodoListRun?> GetTodoListRunByRunIdAsync(Guid runId, int itemId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoListRun>> GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TodoListRun>> GetRunsDueForResetAsync(CancellationToken cancellationToken = default);
    }
}
