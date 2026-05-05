using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoItemTemplate : EntityBase
    {
public TodoListId TodoListId { get; private set; }
public TodoItemDescription Description { get; private set; }
        public bool IsDeleted { get; private set; }

        public TodoList TodoList { get; private set; }
    }
}
