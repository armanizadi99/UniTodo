using System.Linq.Expressions;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface IRepositoryWithTypedId<TEntity, TEntityId>
where TEntity : EntityBase<TEntityId>
    {
        Task<TEntity?> GetByIdAsync(TEntityId id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);

        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Remove(TEntity entity);
    }
}
