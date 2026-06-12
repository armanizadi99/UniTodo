using Microsoft.VisualBasic;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class TodoItem : EntityBase
    {
        public int RunId { get; private set; }
        public TodoItemDescription Description { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public UserId? CompletedBy { get; private set; }
        public TodoItemNotes? Notes { get; private set; }
        public UserId? AssignedTo { get; private set; }

        public TodoListRun Run { get; private set; } = null!;

        private TodoItem() { }

        public TodoItem(TodoItemDescription description)
        {
            Description = description;
        }

        public Result MarkComplete(UserId actorId)
        {
            if (CompletedAt != null)
                return DomainError.InvalidOperation("This item is already marked complete.");
            if (AssignedTo == null && actorId != Run.ownerId)
                return DomainError.NotAuthorized();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            IsCompleted = true;
            CompletedAt = DateTimeOffset.UtcNow;
            CompletedBy = actorId;
            return Result.Success();
        }

        public Result MarkIncomplete(UserId actorId)
        {
            if (CompletedAt == null)
                return DomainError.InvalidOperation("This item is still incomplete.");
            if (AssignedTo == null && actorId != Run.ownerId)
                return DomainError.NotAuthorized();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            IsCompleted = false;
            CompletedAt = null;
            CompletedBy = null;
            return Result.Success();
        }

        public Result UpdateNotes(TodoItemNotes notes, UserId actorId)
        {
            if (AssignedTo == null && actorId != Run.ownerId)
                return DomainError.NotAuthorized();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                return DomainError.NotAuthorized();
            Notes = string.IsNullOrEmpty(notes.Value) ? null : notes;
            return Result.Success();
        }

        public Result ChangeDescription(TodoItemDescription description)
        {
            Description = description;
            return Result.Success();
        }

        public Result AssignTo(UserId assignedTo)
        {
            if (IsCompleted)
                return DomainError.InvalidOperation("Couldn't asign a completed task.");
            AssignedTo = assignedTo;
            return Result.Success();
        }

        public Result AssignToNoone()
        {
            AssignedTo = null;
            return Result.Success();
        }
    }
}
