using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq.Expressions;
using UniTodo.Modules.Todos.Application.Common;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class RepositoryExtensions
    {
        public static async Task<TEntity> GetByIdOrThrowAsync<TEntity, TEntityId>(
            this IRepository<TEntity, TEntityId> repository,
            int id,
            params Expression<Func<TEntity, object>>[] includes )
            where TEntity : EntityBase<TEntityId>
where TEntityId : IStronglyTypedId<int>
        {
        var entity = await repository.GetAsync(e => e.Id.Value == id, includes);
        if (entity is null)
            throw new DomainEntityNotFoundException(typeof(TEntity).Name, id);

        return entity;
        }
    }
}