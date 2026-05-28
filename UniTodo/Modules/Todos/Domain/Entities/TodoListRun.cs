using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    internal class TodoListRun : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();
        private readonly List<UserId> _members = new List<UserId>();

internal Guid RunId { get; private set; }
        internal ResetPolicy ResetPolicy { get; private set; }
        internal string Name { get; private set; }
        internal UserId ownerId { get; private set; }
        internal TodoListRunStatus Status { get; private set; }
        internal DateTimeOffset? ClosedAt { get; private set; }

        internal IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
        internal IReadOnlyCollection<UserId> Members => _members.AsReadOnly();

internal TodoListRun(string name, ResetPolicy resetPolicy, UserId ownerUserId)
{
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        if (!ResetPolicy.IsDefined(resetPolicy))
            throw new ArgumentException("Provided value is undefined.", nameof(resetPolicy));
        Name = name;
ResetPolicy = resetPolicy;
ownerId = ownerUserId;
        RunId = Guid.NewGuid();
        Status = TodoListRunStatus.Active;
        }

internal static TodoListRun CreateRunFromTodoItemTemplates(IEnumerable<TodoItemTemplate> itemTemplates, string name, ResetPolicy resetPolicy, UserId ownerUserId)
{
        var todoListRun = new TodoListRun(name, resetPolicy, ownerUserId);
        todoListRun._todoItems.AddRange(
itemTemplates.Select(t =>  new TodoItem(t.Description)));
        return todoListRun;
        }
    }
}
