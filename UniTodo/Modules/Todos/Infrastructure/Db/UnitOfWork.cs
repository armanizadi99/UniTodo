using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TodoDbContext _context;

        public UnitOfWork( TodoDbContext context )
        {
_context = context;
        }

public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
{

        return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
