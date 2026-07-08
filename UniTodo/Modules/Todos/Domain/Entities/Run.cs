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
        public RunSettings Settings { get; private set; }
        public RunPermissions Permissions { get; private set; }

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
            Settings = Defaults.DefaultRunSettings;
            Permissions = Defaults.DefaultRunPermissions;
            SetResetPolicy(resetPolicy);
        }

        public Result<RunSettings> UpdateSettings(RunSettings settings, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run's settings cannot be updated.");
            Settings = settings;
            UpdateResetsAt();
            return Settings;
        }

        public Result<RunPermissions> UpdatePermissions(RunPermissions permissions, UserId actorId)
        {
            if (actorId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run's permissions cannot be updated.");
            Permissions = permissions;
            return Permissions;
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
            UpdateResetsAt();
        }

        private void UpdateResetsAt()
        {
            var now = DateTime.UtcNow;
            var userNow = TimeZoneInfo.ConvertTimeFromUtc(now, Settings.TimeZone);
            var userTomorrow = new DateTime(userNow.Year, userNow.Month, userNow.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1);
            var userNextMonth = new DateTime(userNow.Year, userNow.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(1);
            ResetsAt = ResetPolicy switch
            {
                ResetPolicy.Daily => new DateTimeOffset(userTomorrow, Settings.TimeZone.GetUtcOffset(userTomorrow)),
                ResetPolicy.Weekly => CalculateNextWeeklyReset(userNow),
                ResetPolicy.Monthly => new DateTimeOffset(userNextMonth, Settings.TimeZone.GetUtcOffset(userNextMonth)),
                _ => null
            };
        }

        private DateTimeOffset CalculateNextWeeklyReset(DateTime userNow)
        {
            DayOfWeek resetDayOfWeek = (DayOfWeek)(((int)Settings.EndOfWeekDay + 1) % 7);
            int daysUntilReset = ((int)resetDayOfWeek - (int)userNow.DayOfWeek + 7) % 7;

            if (daysUntilReset == 0)
            {
                daysUntilReset = 7;
            }

            DateTime targetLocalMidnight = new DateTime(userNow.Year, userNow.Month, userNow.Day, 0, 0, 0, DateTimeKind.Unspecified).AddDays(daysUntilReset);

            return new DateTimeOffset(targetLocalMidnight, Settings.TimeZone.GetUtcOffset(targetLocalMidnight));
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

            var result = CurrentIteration.Close();
            if (!result.IsSuccess)
                return result;

            var newIteration = new RunIteration();
            foreach (var item in CurrentIteration.RunItems)
            {
                var addResult = newIteration.AddItem(new RunItem(item.Description));
                if (!addResult.IsSuccess)
                    return addResult;
            }
            if (Settings.PreserveHistory is false)
                _iterations.Remove(CurrentIteration);

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
            if (!Permissions.MemberAllowedToAddItems && actorUserId != ownerId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be added to a closed run.");
            if (CurrentIteration.RunItems.Any(i => String.Equals(i.Description.Value, item.Description.Value, StringComparison.OrdinalIgnoreCase)))
                return DomainError.DuplicateEntities("No duplicate description could be in a run.");
            var result = CurrentIteration.AddItem(item);
            if (!result.IsSuccess)
                return result;

            return Result.Success();
        }

        public Result DeleteItem(int itemId, UserId actorId)
        {
            if (!Permissions.MemberAllowedToRemoveItems && ownerId != actorId)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("Items couldn't be deleted from a closed run.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            var result = CurrentIteration.RemoveItem(item);
            if (!result.IsSuccess)
                return result;

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
            if (item.AssignedTo == null && actorId != ownerId && !Permissions.MemberAllowedToCompleteUnassignedItems)
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
            if (item.AssignedTo == null && actorId != ownerId && !Permissions.MemberAllowedToMarkIncompleteUnassignedItems)
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
            if (item.AssignedTo == null && actorId != ownerId && !Permissions.MemberAllowedToModifyNotesForUnassignedItems)
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
            if (actorId != ownerId && !Permissions.MemberAllowedToChangeDescriptions)
                return DomainError.NotAuthorized();
            if (Status == TodoListRunStatus.Closed)
                return DomainError.InvalidOperation("A closed run couldn't get modified.");
            var item = CurrentIteration.RunItems.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
                return DomainError.EntityNotFound(nameof(RunItem), itemId);
            if (CurrentIteration.RunItems.Any(i => i.Id != itemId && string.Equals(i.Description.Value, description.Value, StringComparison.OrdinalIgnoreCase)))
                return DomainError.DuplicateEntities("No duplicate description could be in a run.");
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
