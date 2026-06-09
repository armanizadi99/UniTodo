using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    public class TodoListRunRepository : Repository<TodoListRun>, ITodoListRunRepository
    {
        private readonly DbSet<TodoListRun> _dbSet;

        public TodoListRunRepository(TodoDbContext context) : base(context)
        {
            _dbSet = context.Set<TodoListRun>();
        }

        async Task<TodoListRun?> ITodoListRunRepository.GetTodoListRunByIdAsync(int id, bool includeItems, CancellationToken cancellationToken)
        {
            IQueryable<TodoListRun> query = _dbSet;

            if (includeItems)
                query = query.Include(i => i.TodoItems);

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        async Task<TodoListRun?> ITodoListRunRepository.GetTodoListRunByIdAsync(int id, int itemId, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(i => i.TodoItems.Where(item => item.Id == itemId))
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        async Task<IReadOnlyList<TodoListRun>> ITodoListRunRepository.GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(e => e.Status == Domain.Enums.TodoListRunStatus.Active)
    .Where(e => e.Members.Select(m => m).Contains(new Domain.ValueObjects.UserId(userId)))
    .ToListAsync(cancellationToken);
        }
    }
}
