using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq.Expressions;
using UniTodo.Modules.Todos.Application.Common;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    internal static class RepositoryExtensions
    {
        internal static async Task<TEntity> GetByIdOrThrowAsync<TEntity>(
            this IRepositoryWithTypedId<TEntity, int> repository,
            int id,
            params Expression<Func<TEntity, object>>[] includes )
            where TEntity : EntityBase<int>
        {
        var entity = await repository.GetAsync(e => e.Id == id, includes);
        if (entity is null)
            throw new DomainEntityNotFoundException(typeof(TEntity).Name, id);

        return entity;
        }
    }
}