using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    internal class TodoListRun : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();

internal ResetPolicy ResetPolicy { get; private set; }
internal string Name { get; private set; }
internal UserId ownerId { get; private set; }
internal TodoListRunStatus Status { get; private set; }
internal DateTimeOffset? ClosedAt{ get; private set; }

        internal IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
    }
}
