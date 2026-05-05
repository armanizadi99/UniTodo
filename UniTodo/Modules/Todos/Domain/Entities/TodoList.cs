using System.Reflection.Metadata;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoList : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();

        public UserId OwnerId { get; private set; }
        public string Name { get; private set; }
        public ResetPolicy ResetPolicy { get; private set; }
        public TodoListStatus Status { get; private set; }

        public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();

public TodoList( UserId ownerId, string name, ResetPolicy resetPolicy)
        {
        if (!ResetPolicy.IsDefined(resetPolicy))
            throw new ArgumentOutOfRangeException($"{nameof(resetPolicy)} is not defined.");
        ArgumentException.ThrowIfNullOrEmpty(name);
        OwnerId = ownerId;
        Name = name;
        ResetPolicy = resetPolicy;
        Status = TodoListStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
