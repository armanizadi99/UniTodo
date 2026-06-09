using System.Security.Cryptography.X509Certificates;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoListRun : EntityBase
    {
        private readonly List<TodoItem> _todoItems = new List<TodoItem>();
        private readonly List<RunMember> _members = new List<RunMember>();

        public Guid RunId { get; private set; }
        public ResetPolicy ResetPolicy { get; private set; }
        public string Name { get; private set; }
        public UserId ownerId { get; private set; }
        public TodoListRunStatus Status { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public bool IsShared { get; private set; }

        public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();
        public IReadOnlyCollection<RunMember> Members => _members.AsReadOnly();

        private TodoListRun() { }

        public TodoListRun(string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            if (!ResetPolicy.IsDefined(resetPolicy))
                throw new ArgumentException("Provided value is undefined.", nameof(resetPolicy));
            Name = name;
            ResetPolicy = resetPolicy;
            ownerId = ownerUserId;
            _members.Add(new RunMember(ownerId));
            RunId = Guid.NewGuid();
            Status = TodoListRunStatus.Active;
            IsShared = isShared;
        }

        public static Result<TodoListRun> CreateRunFromTodoItemTemplates(IEnumerable<TodoItemTemplate> itemTemplates, string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            var todoListRun = new TodoListRun(name, resetPolicy, isShared, ownerUserId);
            foreach (var template in itemTemplates)
            {
                var result = todoListRun.AddTodoItem(new TodoItem(template.Description), ownerUserId);
        if (!result.IsSuccess)
            return Result<TodoListRun>.Failure(result.Error);
            }
            return todoListRun;
        }

        public Result AddTodoItem(TodoItem item, UserId actorUserId)
        {
        if (actorUserId != ownerId)
            return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be added to a closed run.");
            if (_todoItems.Any(i => String.Equals(i.Description.Value, item.Description.Value, StringComparison.OrdinalIgnoreCase)))
                return DomainError.DuplicateEntities("No duplicate description could be in a todo list run.");
            _todoItems.Add(item);
        return Result.Success();
        }

        public Result DeleteItem(int itemId, UserId actorId)
        {
        if (ownerId != actorId)
            return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be deleted from a closed run.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            _todoItems.Remove(item);
        return Result.Success();
        }

        public Result MakeShared(UserId actorUserId)
        {
            if (actorUserId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (IsShared)
                return DomainError.InvalidOperation("This run is already shared.");
            IsShared = true;
        return Result.Success();
        }

        public Result MakePrivate(UserId actorUserId)
        {
            if (actorUserId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (!IsShared)
                return DomainError.InvalidOperation("This run is already private.");
            _members.RemoveAll(m => !m.UserId.Equals(ownerId));
            foreach (var item in _todoItems)
            {
                var result = item.AssignToNoone();
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
            }
            IsShared = false;
        return Result.Success();
        }

        public Result MarkItemComplete(int itemId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var itemToMarkComplete = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToMarkComplete is null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            var result = itemToMarkComplete.MarkComplete(actorId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
        return Result.Success();
        }

        public Result MarkItemIncomplete(int itemId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var itemToMarkIncomplete = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToMarkIncomplete is null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            var result = itemToMarkIncomplete.MarkIncomplete(actorId);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
        return Result.Success();
        }

        public Result UpdateNotes(int itemId, TodoItemNotes notes, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            var result = item.UpdateNotes(notes, actorId);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
        return Result.Success();
        }

        public Result AssignItemToMember(int itemId, UserId memberId, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (!_members.Any(m => m.UserId.Equals(memberId)))
                return DomainError.InvalidOperation("An item couldn't get asigned to someone that is not a member of the run.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            var result = item.AssignTo(memberId);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
return Result.Success();
        }

        public Result  ChangeItemDescription(int itemId, TodoItemDescription description, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = _todoItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(TodoItem), itemId);
            var result = item.ChangeDescription(description);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
return Result.Success();
        }

        public Result<RunMember> AddMember(UserId userId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (!IsShared)
                return DomainError.InvalidOperation("Couldn't add members to a private group.");
            if (_members.Any(m => m.UserId.Equals(userId)))
                return DomainError.DuplicateEntities("this user is already a member of this run");
        var member = new RunMember(userId);
            _members.Add(member);
            return member;
        }

        public Result RemoveMember(UserId userId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (userId == ownerId)
                return DomainError.InvalidOperation("Owner of a run couldn't be get removed.");
            if (!_members.Any(m => m.UserId.Equals(userId)))
                return DomainError.InvalidOperation("This user is not a member of this run.");
            foreach (var item in _todoItems)
            {
        if (item.AssignedTo == userId)
        {
        var result = item.AssignToNoone();
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
        }
            }
            _members.RemoveAll(m =>  m.UserId.Equals(userId));
        return Result.Success();
        }
    }
}