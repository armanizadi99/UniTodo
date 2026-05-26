using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    internal class RepositoryWithTypedId<TEntity, TEntityId> : IRepositoryWithTypedId<TEntity, TEntityId>
where TEntity : EntityBase<TEntityId>
    {
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryWithTypedId(TodoDbContext context)
        {
            _dbSet = context.Set<TEntity>();
        }

        async Task<TEntity?> IRepositoryWithTypedId<TEntity, TEntityId>.GetByIdAsync(
        TEntityId id,
        CancellationToken cancellation,
        params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            // C# couldn't make sure that the TEntityId have equality operator, so we'll use a trick here.
            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        async Task<List<TEntity>> IRepositoryWithTypedId<TEntity, TEntityId>.GetListAsync(
        Expression<Func<TEntity, bool>>? filter,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync();
        }

        async Task IRepositoryWithTypedId<TEntity, TEntityId>.AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        void IRepositoryWithTypedId<TEntity, TEntityId>.Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        void IRepositoryWithTypedId<TEntity, TEntityId>.Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
