using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoItemTemplate : EntityBase
    {
        public int TodoListId { get; private set; }
        public TodoItemDescription Description { get; private set; }
        public TodoListTemplate TodoList { get; private set; }

        public TodoItemTemplate(int todoListId, TodoItemDescription description)
        {
            TodoListId = todoListId;
            Description = description;
        }
    }
}
