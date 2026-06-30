using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    public class RunRepository : Repository<Run>, IRunRepository
    {
        private readonly DbSet<Run> _dbSet;

        public RunRepository(TodoDbContext context) : base(context)
        {
            _dbSet = context.Set<Run>();
        }

        async Task<Run?> IRunRepository.GetRunByIdAsync(int id, bool includeItems, CancellationToken cancellationToken)
        {
            IQueryable<Run> query = _dbSet;

            if (includeItems)
                query = query.Include(r => r.Iterations.OrderByDescending(i => i.Id).Take(1))
                .ThenInclude(i => i.RunItems);
            else
                query = query.Include(r => r.Iterations.OrderByDescending(i => i.Id).Take(1));

            // AsSplitQuery avoids combining the filtered Iterations include with the Members include
            // into a single query, which would require SQL APPLY (unsupported on SQLite).
            query = query.Include(r => r.Members).AsSplitQuery();

            return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        async Task<Run?> IRunRepository.GetRunByIdAsync(int id, int itemId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(r => r.Iterations.OrderByDescending(i => i.Id).Take(1))
                .ThenInclude(i => i.RunItems.Where(item => item.Id == itemId))
                .Include(r => r.Members)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        async Task<IReadOnlyList<Run>> IRunRepository.GetUserActiveRunsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Include(r => r.Members)
.Where(e => e.Status == Domain.Enums.TodoListRunStatus.Active)
    .Where(e => e.Members.Select(m => m.UserId).Contains(new Domain.ValueObjects.UserId(userId)))
    .ToListAsync(cancellationToken);
        }

        async Task<Run?> IRunRepository.GetRunWithAllIterationsAsync(int id, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(r => r.Iterations)
                .ThenInclude(i => i.RunItems)
                .Include(r => r.Members)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        async Task<IReadOnlyList<Run>> IRunRepository.GetRunsDueForResetAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            // Using FromSqlInterpolated to perform a single-hit, server-side filtered query.
            // This bypasses the EF Core translation issue with DateTimeOffset in SQLite
            // while remaining efficient by not fetching unnecessary records or hitting the DB twice.
            return await _dbSet
                .FromSqlInterpolated($@"
                    SELECT * FROM runs
                    WHERE Status = {(int)Domain.Enums.TodoListRunStatus.Active}
                      AND ResetPolicy <> {(int)Domain.Enums.ResetPolicy.None}
                      AND ResetsAt IS NOT NULL
                      AND ResetsAt <= {now}")
                .Include(r => r.Iterations.OrderByDescending(i => i.Id).Take(1))
                .ThenInclude(i => i.RunItems)
                .Include(r => r.Members)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);
        }
    }
}
