namespace UniTodo.Modules.Todos.Application.Interfaces
{
    internal interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
