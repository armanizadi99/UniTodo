using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    internal class RepositoryWithTypedId<TEntity, TEntityId> : IRepositoryWithTypedId<TEntity, TEntityId>
where TEntity : EntityBase<TEntityId>
    {
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryWithTypedId( TodoDbContext context )
        {
        _dbSet = context.Set<TEntity>();
        }

async Task<TEntity?> IRepositoryWithTypedId<TEntity, TEntityId>.GetAsync(
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

async Task<List<TEntity>> IRepositoryWithTypedId<TEntity, TEntityId>.GetListAsync(
Expression<Func<TEntity, bool>>? filter,
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
