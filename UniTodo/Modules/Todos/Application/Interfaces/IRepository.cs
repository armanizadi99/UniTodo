using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    public interface IRepository<TEntity> : IRepositoryWithTypedId<TEntity, int>
where TEntity : EntityBase<int>
    {
    }
}
