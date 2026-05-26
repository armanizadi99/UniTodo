using System.Reflection.Metadata;
using UniTodo.Modules.Todos.Application.Common;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    internal class TodoListTemplate : EntityBase
    {
        private readonly List<TodoItemTemplate> _todoItemTemplates = new List<TodoItemTemplate>();

        internal UserId OwnerId { get; private set; }
        internal string Name { get; private set; }
        internal ResetPolicy ResetPolicy { get; private set; }
        internal TodoListStatus Status { get; private set; }

        internal IReadOnlyCollection<TodoItemTemplate> TodoItemTemplates => _todoItemTemplates.AsReadOnly();

        internal TodoListTemplate(UserId ownerId, string name, ResetPolicy resetPolicy)
        {
            if (!ResetPolicy.IsDefined(resetPolicy))
                throw new ArgumentOutOfRangeException($"{nameof(resetPolicy)} is not defined.");
            ArgumentException.ThrowIfNullOrEmpty(name);
            OwnerId = ownerId;
            Name = name;
            ResetPolicy = resetPolicy;
            Status = TodoListStatus.Active;
        }

        internal void Archive(UserId actorUserId)
        {
            if (OwnerId != actorUserId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListStatus.Archived)
                throw new DomainInvalidOperationException("This todo list is already archived.");
            Status = TodoListStatus.Archived;
        }

        internal void MakeActive(UserId actorUserId)
        {
            if (OwnerId != actorUserId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListStatus.Active)
                throw new DomainInvalidOperationException("This todo list is already active.");
            Status = TodoListStatus.Active;
        }

        internal void AddTodoItemTemplate(TodoItemTemplate todoItemTemplate, UserId actorUserId)
        {
            if (OwnerId != actorUserId)
                throw new DomainNotAuthorizedException();
            if (TodoItemTemplates.Any(e => e.Description.Value.Equals(todoItemTemplate.Description.Value, StringComparison.OrdinalIgnoreCase)))
                throw new DomainDuplicateEntitiesException("No duplicate descriptions allowed in a TodoList.");
            _todoItemTemplates.Add(todoItemTemplate);
        }

        internal void Delete(int id, UserId actorId)
        {
            if (OwnerId != actorId)
                throw new DomainNotAuthorizedException();
            var todoItemTemplate = _todoItemTemplates.Where(t => t.Id == id).FirstOrDefault();
            if (todoItemTemplate is null)
                throw new DomainEntityNotFoundException("TodoItemTemplate doesn't exist", id);
            _todoItemTemplates.Remove(todoItemTemplate);
        }
    }
}
