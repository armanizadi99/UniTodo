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

        async Task<TodoListRun?> ITodoListRunRepository.GetTodoListRunByRunIdAsync(Guid runId, bool includeItems, CancellationToken cancellationToken)
        {
            IQueryable<TodoListRun> query = _dbSet;

            if (includeItems)
                query = query.Include(i => i.TodoItems);

            query = query.Include(i => i.Members);

            return await query.FirstOrDefaultAsync(e => e.RunId == runId);
        }

        async Task<TodoListRun?> ITodoListRunRepository.GetTodoListRunByRunIdAsync(Guid runId, int itemId, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(i => i.TodoItems.Where(item => item.Id == itemId))
.Include(i => i.Members)
            .FirstOrDefaultAsync(e => e.RunId == runId, cancellationToken);
        }

        async Task<IReadOnlyList<TodoListRun>> ITodoListRunRepository.GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(i => i.Members)
.Where(e => e.Status == Domain.Enums.TodoListRunStatus.Active)
    .Where(e => e.Members.Select(m => m.UserId).Contains(new Domain.ValueObjects.UserId(userId)))
    .ToListAsync(cancellationToken);
        }

        async Task<IReadOnlyList<TodoListRun>> ITodoListRunRepository.GetRunsDueForResetAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            // Using FromSqlInterpolated to perform a single-hit, server-side filtered query.
            // This bypasses the EF Core translation issue with DateTimeOffset in SQLite 
            // while remaining efficient by not fetching unnecessary records or hitting the DB twice.
            return await _dbSet
                .FromSqlInterpolated($@"
                    SELECT * FROM todoListRuns 
                    WHERE Status = {(int)Domain.Enums.TodoListRunStatus.Active} 
                      AND ResetPolicy <> {(int)Domain.Enums.ResetPolicy.None} 
                      AND ResetsAt IS NOT NULL 
                      AND ResetsAt <= {now}")
                .Include(r => r.TodoItems)
                .Include(r => r.Members)
                .ToListAsync(cancellationToken);
        }
    }
}
