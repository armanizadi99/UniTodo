using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class RunItem : EntityBase
    {
        public int RunIterationId { get; private set; }
        public TodoItemDescription Description { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public UserId? CompletedBy { get; private set; }
        public TodoItemNotes? Notes { get; private set; }
        public UserId? AssignedTo { get; private set; }

        public RunIteration Iteration { get; private set; } = null!;

        private RunItem() { }

        public RunItem(TodoItemDescription description)
        {
            Description = description;
        }

        public Result MarkComplete(UserId actorId)
        {
            if (CompletedAt != null)
                return DomainError.InvalidOperation("This item is already marked complete.");
            IsCompleted = true;
            CompletedAt = DateTimeOffset.UtcNow;
            CompletedBy = actorId;
            return Result.Success();
        }

        public Result MarkIncomplete()
        {
            if (CompletedAt == null)
                return DomainError.InvalidOperation("This item is still incomplete.");
            IsCompleted = false;
            CompletedAt = null;
            CompletedBy = null;
            return Result.Success();
        }

        public Result UpdateNotes(TodoItemNotes notes)
        {
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
