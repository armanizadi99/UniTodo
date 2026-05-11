using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoItemTemplate : EntityBase<TodoItemTemplateId>
    {
public TodoListTemplateId TodoListId { get; private set; }
public TodoItemDescription Description { get; private set; }

        public TodoListTemplate TodoList { get; private set; }

public TodoItemTemplate( TodoListTemplateId todoListId, TodoItemDescription description )
        {
        TodoListId = todoListId;
        Description = description;
        }
    }
}
