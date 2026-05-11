using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoListRun : EntityBase<TodoListRunId>
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();

public ResetPolicy ResetPolicy { get; private set; }
public string Name { get; private set; }
public UserId ownerId { get; private set; }
public TodoListRunStatus Status { get; private set; }
public DateTimeOffset? ClosedAt{ get; private set; }

        public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
    }
}
