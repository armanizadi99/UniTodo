using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class TodoListRunRepositoryExtensions
    {
        public static async Task<TodoListRun> GetTodoListRunByIdOrThrowAsync(this ITodoListRunRepository repository, int id, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            var run = await repository.GetTodoListRunByIdAsync(id, includeItems, cancellationToken);
            if (run is null)
                throw new DomainEntityNotFoundException(nameof(TodoListRun), id);
            return run;
        }

        public static async Task<TodoListRun> GetTodoListRunByIdOrThrowAsync(this ITodoListRunRepository repository, int id, int itemId, CancellationToken cancellationToken = default)
        {
            var run = await repository.GetTodoListRunByIdAsync(id, itemId, cancellationToken);
            if (run is null)
                throw new DomainEntityNotFoundException(nameof(TodoListRun), id);
            return run;
        }
    }
}
