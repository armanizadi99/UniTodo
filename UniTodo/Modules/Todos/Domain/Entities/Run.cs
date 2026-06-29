using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class Run : EntityBase
    {
        private readonly List<RunIteration> _iterations = new List<RunIteration>();
        private readonly List<RunMember> _members = new List<RunMember>();

        public ResetPolicy ResetPolicy { get; private set; }
        public string Name { get; private set; }
        public UserId ownerId { get; private set; }
        public TodoListRunStatus Status { get; private set; }
        public DateTimeOffset? ClosedAt { get; private set; }
        public DateTimeOffset? ResetsAt { get; private set; }
        public bool IsShared { get; private set; }

        public IReadOnlyCollection<RunIteration> Iterations => _iterations.AsReadOnly();
        public IReadOnlyCollection<RunMember> Members => _members.AsReadOnly();

        public RunIteration CurrentIteration => _iterations[^1];

        private Run() { }

        public Run(string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
            if (!ResetPolicy.IsDefined(resetPolicy))
                throw new ArgumentException("Provided value is undefined.", nameof(resetPolicy));
            Name = name;
            ownerId = ownerUserId;
            _members.Add(new RunMember(ownerId));
            _iterations.Add(new RunIteration());
            Status = TodoListRunStatus.Active;
            IsShared = isShared;
            SetResetPolicy(resetPolicy);
        }

        public Result UpdateResetPolicy(ResetPolicy newPolicy, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run's policy cannot be updated.");

            SetResetPolicy(newPolicy);
            return Result.Success();
        }

        private void SetResetPolicy(ResetPolicy newPolicy)
        {
            ResetPolicy = newPolicy;
            var now = DateTimeOffset.UtcNow;
            var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
            ResetsAt = ResetPolicy switch
            {
                ResetPolicy.Daily => today.AddDays(1),
                ResetPolicy.Weekly => GetNextSaturday(today),
                ResetPolicy.Monthly => new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset).AddMonths(1),
                _ => null
            };
        }

        private DateTimeOffset GetNextSaturday(DateTimeOffset today)
        {
            int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            if (daysUntilSaturday == 0) daysUntilSaturday = 7;
            return today.AddDays(daysUntilSaturday);
        }

        public Result Close(UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("The run is already closed.");

            Status = TodoListRunStatus.Closed;
            ClosedAt = DateTimeOffset.UtcNow;
            return Result.Success();
        }

        public Result Reset(UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            return ResetInternal();
        }

        public Result Reset()
        {
            return ResetInternal();
        }

        private Result ResetInternal()
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run cannot be reset.");

            if (ResetPolicy != ResetPolicy.None)
            {
                if (DateTimeOffset.UtcNow < ResetsAt)
                    return DomainError.InvalidOperation("The run cannot be reset before the scheduled time.");
            }

            var newIteration = new RunIteration();
            foreach (var item in CurrentIteration.RunItems)
            {
                newIteration.AddItem(new RunItem(item.Description));
            }
            _iterations.Add(newIteration);

            SetResetPolicy(ResetPolicy);

            return Result.Success();
        }

        public static Result<Run> CreateRunFromRunItemTemplates(IEnumerable<TodoItemTemplate> itemTemplates, string name, ResetPolicy resetPolicy, bool isShared, UserId ownerUserId)
        {
            var run = new Run(name, resetPolicy, isShared, ownerUserId);
            foreach (var template in itemTemplates)
            {
                var result = run.AddRunItem(new RunItem(template.Description), ownerUserId);
                if (!result.IsSuccess)
                    return Result<Run>.Failure(result.Error);
            }
            return run;
        }

        public Result AddRunItem(RunItem item, UserId actorUserId)
        {
            if (actorUserId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be added to a closed run.");
            if (CurrentIteration.RunItems.Any(i => String.Equals(i.Description.Value, item.Description.Value, StringComparison.OrdinalIgnoreCase)))
                return DomainError.DuplicateEntities("No duplicate description could be in a run.");
            CurrentIteration.AddItem(item);
            return Result.Success();
        }

        public Result DeleteItem(int itemId, UserId actorId)
        {
            if (ownerId != actorId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be deleted from a closed run.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            CurrentIteration.RemoveItem(item);
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
            foreach (var item in CurrentIteration.RunItems)
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
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            if (item.AssignedTo == null && actorId != ownerId)
                return DomainError.NotAuthorized();
            if (item.AssignedTo != null && item.AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            return item.MarkComplete(actorId);
        }

        public Result MarkItemIncomplete(int itemId, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            if (item.AssignedTo == null && actorId != ownerId)
                return DomainError.NotAuthorized();
            if (item.AssignedTo != null && item.AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            return item.MarkIncomplete();
        }

        public Result UpdateNotes(int itemId, TodoItemNotes notes, UserId actorId)
        {
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            if (item.AssignedTo == null && actorId != ownerId)
                return DomainError.NotAuthorized();
            if (item.AssignedTo != null && item.AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            return item.UpdateNotes(notes);
        }

        public Result AssignItemToMember(int itemId, UserId memberId, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            if (!_members.Any(m => m.UserId.Equals(memberId)))
                return DomainError.InvalidOperation("An item couldn't get asigned to someone that is not a member of the run.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            return item.AssignTo(memberId);
        }

        public Result ChangeItemDescription(int itemId, TodoItemDescription description, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            return item.ChangeDescription(description);
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
            foreach (var item in CurrentIteration.RunItems)
            {
                if (item.AssignedTo == userId)
                {
                    var result = item.AssignToNoone();
                    if (!result.IsSuccess)
                        return Result.Failure(result.Error);
                }
            }
            _members.RemoveAll(m => m.UserId.Equals(userId));
            return Result.Success();
        }
    }
}
