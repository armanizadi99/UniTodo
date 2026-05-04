using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoListRun : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();

public TodoListId TodoListId { get; private set; }
public DateTime PeriodStart{ get; private set; }
public DateTime PeriodEnd { get; private set; }
public TodoListRunStatus Status { get; private set; }
public DateTime? ClosedAt{ get; private set; }

        public TodoList TodoList { get; private set; }

        public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
    }
}
