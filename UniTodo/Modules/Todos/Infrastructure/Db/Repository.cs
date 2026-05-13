using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Infrastructure.Db
{
    public class Repository<TEntity> : RepositoryWithTypedId<TEntity, int>
where TEntity : EntityBase<int>
    {
public Repository( TodoDbContext context ) : base(context)
        {
        }
    }
}
