using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly TodoDbContext _context;

        public UnitOfWork(TodoDbContext context)
        {
            _context = context;
        }

        async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
