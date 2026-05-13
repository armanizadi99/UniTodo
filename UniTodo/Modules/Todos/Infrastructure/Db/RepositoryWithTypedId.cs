using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    public class RepositoryWithTypedId<TEntity, TEntityId> : IRepositoryWithTypedId<TEntity, TEntityId>
where TEntity : EntityBase<TEntityId>
    {
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryWithTypedId( TodoDbContext context )
        {
        _dbSet = context.Set<TEntity>();
        }

public async Task<TEntity?> GetAsync(
Expression<Func<TEntity, bool>> filter,
params Expression<Func<TEntity, object>>[] includes)
{
        IQueryable<TEntity> query = _dbSet;

foreach(var include in includes)
{
        query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(filter);
        }

public async Task<List<TEntity>> GetListAsync(
Expression<Func<TEntity, bool>>? filter = null,
params Expression<Func<TEntity, object>>[] includes)
{
        IQueryable<TEntity> query = _dbSet;

foreach (var include in includes)
{
query = query.Include(include);
        }
if(filter != null)
        query = query.Where(filter);

return await query.ToListAsync();
        }

public async Task AddAsync(TEntity entity)
{
await _dbSet.AddAsync(entity);
        }

public void Update(TEntity entity)
{
_dbSet.Update(entity);
        }

public void Remove(TEntity entity)
{
_dbSet.Remove(entity);
        }
    }
}
