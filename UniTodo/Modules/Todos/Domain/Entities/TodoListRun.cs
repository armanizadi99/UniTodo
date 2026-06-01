using System.Security.Cryptography.X509Certificates;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoListRun : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();
        private readonly List<UserId> _members = new List<UserId>();

        public Guid RunId { get; private set; }
        public ResetPolicy ResetPolicy { get; private set; }
        public string Name { get; private set; }
        public UserId ownerId { get; private set; }
        public TodoListRunStatus Status { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public bool IsShared { get; private set; }

        public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
        public IReadOnlyCollection<UserId> Members => _members.AsReadOnly();

        public TodoListRun(string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            if (!ResetPolicy.IsDefined(resetPolicy))
                throw new ArgumentException("Provided value is undefined.", nameof(resetPolicy));
            Name = name;
            ResetPolicy = resetPolicy;
            ownerId = ownerUserId;
            _members.Add(ownerId);
            RunId = Guid.NewGuid();
            Status = TodoListRunStatus.Active;
            IsShared = isShared;
        }

        public static TodoListRun CreateRunFromTodoItemTemplates(IEnumerable<TodoItemTemplate> itemTemplates, string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            var todoListRun = new TodoListRun(name, resetPolicy, isShared, ownerUserId);
            foreach (var template in itemTemplates)
            {
                todoListRun.AddTodoItem(new TodoItem(template.Description), ownerUserId);
            }
            return todoListRun;
        }

        public void AddTodoItem(TodoItem item, UserId actorUserId)
        {
            if (actorUserId != ownerId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("Items couldn't be added to a closed run.");
            if (_todoItems.Any(i => String.Equals(i.Description.Value, item.Description.Value, StringComparison.OrdinalIgnoreCase)))
                throw new DomainDuplicateEntitiesException("No duplicate description could be in a todo list run.");

            // Use reflection to set the private 'Run' property if needed, 
            // but since it's in the same assembly/module we can just set it if we make it internal or use a trick.
            // Actually, Run property is public.
            typeof(TodoItem).GetProperty(nameof(TodoItem.Run))!.SetValue(item, this);

            _todoItems.Add(item);
        }

        public void DeleteItem(int itemId, UserId actorId)
        {
            if (ownerId != actorId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("Items couldn't be deleted from a closed run.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            _todoItems.Remove(item);
        }

        public void MakeShared(UserId actorUserId)
        {
            if (actorUserId != ownerId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            if (IsShared)
                throw new DomainInvalidOperationException("This run is already shared.");
            IsShared = true;
        }

        public void MakePrivate(UserId actorUserId)
        {
            if (actorUserId != ownerId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            if (!IsShared)
                throw new DomainInvalidOperationException("This run is already private.");
            IsShared = false;
        }

        public void MarkItemComplete(int itemId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            var itemToMarkComplete = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToMarkComplete is null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            itemToMarkComplete.MarkComplete(actorId);
        }

        public void MarkItemIncomplete(int itemId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            var itemToMarkIncomplete = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToMarkIncomplete is null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            itemToMarkIncomplete.MarkIncomplete(actorId);
        }

        public void UpdateNotes(int itemId, TodoItemNotes notes, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            item.UpdateNotes(notes, actorId);
        }

        public void AsignItemToMember(int itemId, UserId memberId, UserId actorId)
        {
            if (actorId != ownerId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            if (!_members.Any(m => m.Equals(memberId)))
                throw new DomainInvalidOperationException("An item couldn't get asigned to someone that is not a member of the run.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            item.SetAsignedTo(memberId);
        }

        public void ChangeItemDescription(int itemId, TodoItemDescription description, UserId actorId)
        {
            if (actorId != ownerId)
                throw new DomainNotAuthorizedException();
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                throw new DomainEntityNotFoundException(nameof(TodoItem), itemId);
            item.ChangeDescription(description);
        }

        public Guid AddMember(UserId userId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            if (actorId != ownerId)
                throw new DomainNotAuthorizedException();
            if (_members.Any(m => m.Equals(userId)))
                throw new DomainDuplicateEntitiesException("this user is already a member of this run");
            _members.Add(userId);
            return userId.Value;
        }

        public void RemoveMember(UserId userId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                throw new DomainInvalidOperationException("A closed run couldn't get modified.");
            if (actorId != ownerId)
                throw new DomainNotAuthorizedException();
            if (!_members.Any(m => m.Equals(userId)))
                throw new DomainInvalidOperationException("This user is not a member of this run.");
            _members.Remove(userId);
        }
    }
}