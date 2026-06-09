using System.Reflection.Metadata;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoListTemplate : EntityBase
    {
        private readonly List<TodoItemTemplate> _todoItemTemplates = new List<TodoItemTemplate>();

        public UserId OwnerId { get; private set; }
        public string Name { get; private set; }
        public ResetPolicy ResetPolicy { get; private set; }
        public TodoListStatus Status { get; private set; }

        public IReadOnlyCollection<TodoItemTemplate> TodoItemTemplates => _todoItemTemplates.AsReadOnly();

        public TodoListTemplate(UserId ownerId, string name, ResetPolicy resetPolicy)
        {
            if (!ResetPolicy.IsDefined(resetPolicy))
                throw new ArgumentOutOfRangeException($"{nameof(resetPolicy)} is not defined.");
            ArgumentException.ThrowIfNullOrEmpty(name);
            OwnerId = ownerId;
            Name = name;
            ResetPolicy = resetPolicy;
            Status = TodoListStatus.Active;
        }

        public Result Archive(UserId actorUserId)
        {
        if (OwnerId != actorUserId)
            return DomainError.NotAuthorized();
            if (Status == TodoListStatus.Archived)
                return DomainError.InvalidOperation("This todo list is already archived.");
            Status = TodoListStatus.Archived;
        return Result.Success();
        }

        public Result MakeActive(UserId actorUserId)
        {
        if (OwnerId != actorUserId)
            return DomainError.NotAuthorized(); ;
            if (Status == TodoListStatus.Active)
                return DomainError.InvalidOperation("This todo list is already active.");
            Status = TodoListStatus.Active;
return Result.Success();
        }

        public Result AddTodoItemTemplate(TodoItemTemplate todoItemTemplate, UserId actorUserId)
        {
            if (OwnerId != actorUserId)
                return DomainError.NotAuthorized();
            if (TodoItemTemplates.Any(e => e.Description.Value.Equals(todoItemTemplate.Description.Value, StringComparison.OrdinalIgnoreCase)))
                return DomainError.DuplicateEntities("No duplicate descriptions allowed in a TodoList.");
            _todoItemTemplates.Add(todoItemTemplate);
return Result.Success();
        }

        public Result Delete(int id, UserId actorId)
        {
        if (OwnerId != actorId)
            return DomainError.NotAuthorized();
            var todoItemTemplate = _todoItemTemplates.Where(t => t.Id == id).FirstOrDefault();
            if (todoItemTemplate is null)
                return DomainError.EntityNotFound(nameof(TodoItemTemplate), id);
            _todoItemTemplates.Remove(todoItemTemplate);
return Result.Success();
        }
    }
}
