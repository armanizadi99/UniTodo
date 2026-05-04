using Microsoft.VisualBasic;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoItem : EntityBase
    {
public TodoListRunId RunId { get; private set; }
public TodoItemTemplateId? TodoItemTemplateId { get; private set; }
public TodoItemDescription description { get; private set; }
public int position { get; private set; }
public bool IsCompleted { get; private set; }
public DateAndTime CompletedAt { get; private set; }
public TodoItemNotes Notes { get; private set; }

public TodoListRun Run { get; private set; }
    }
}
