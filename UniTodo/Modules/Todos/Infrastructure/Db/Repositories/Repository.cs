using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Infrastructure.Db.Repositories
{
    public class Repository<TEntity> : RepositoryWithTypedId<TEntity, int>, IRepository<TEntity>
where TEntity : EntityBase<int>
    {
        public Repository(TodoDbContext context) : base(context)
        {
        }
    }
}
