using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    internal class TodoItemTemplate : EntityBase
    {
internal int TodoListId { get; private set; }
internal TodoItemDescription Description { get; private set; }

        internal TodoListTemplate TodoList { get; private set; }

internal TodoItemTemplate( int todoListId, TodoItemDescription description )
        {
        TodoListId = todoListId;
        Description = description;
        }
    }
}
