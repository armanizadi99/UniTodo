using Microsoft.VisualBasic;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    internal class TodoItem : EntityBase
    {
        internal int RunId { get; private set; }
        internal TodoItemDescription Description { get; private set; }
        internal bool IsCompleted { get; private set; }
        internal DateTimeOffset? CompletedAt { get; private set; }
        internal TodoItemNotes? Notes { get; private set; }

        internal TodoListRun Run { get; private set; } = null!;

internal TodoItem( TodoItemDescription description)
        {
        Description = description;
        }
    }
}
