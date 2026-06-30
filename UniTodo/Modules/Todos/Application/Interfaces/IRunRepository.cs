using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface IRunRepository : IRepository<Run>
    {
        Task<Run?> GetRunByIdAsync(int id, bool includeItems = false, CancellationToken cancellationToken = default);
        Task<Run?> GetRunByIdAsync(int id, int itemId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Run>> GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Run>> GetRunsDueForResetAsync(CancellationToken cancellationToken = default);
        Task<Run?> GetRunWithAllIterationsAsync(int id, CancellationToken cancellationToken = default);
    }
}
