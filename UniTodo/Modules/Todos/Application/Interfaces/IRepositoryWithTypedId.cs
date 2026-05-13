using System.Linq.Expressions;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface IRepositoryWithTypedId<TEntity, TEntityId> 
where TEntity : EntityBase<TEntityId>
    {
        Task<TEntity?> GetAsync(
            Expression<Func
<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] includes );

        Task<List<TEntity>> GetListAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            params Expression<Func<TEntity, object>>[] includes );

        Task AddAsync( TEntity entity );
        void Update( TEntity entity );
        void Remove( TEntity entity );
    }
}
