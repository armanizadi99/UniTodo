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
public UserId? CompletedBy {get; private set; }
        public TodoItemNotes? Notes { get; private set; }
        public UserId? AssignedTo { get; private set; }

        public TodoListRun Run { get; private set; } = null!;

        private TodoItem() { }

        public TodoItem(TodoItemDescription description)
        {
            Description = description;
        }

        public void MarkComplete(UserId actorId)
        {
            if (CompletedAt != null)
                throw new DomainInvalidOperationException("This item is already marked complete.");
            if (AssignedTo == null && actorId != Run.ownerId)
                throw new DomainNotAuthorizedException();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                throw new DomainNotAuthorizedException();
            IsCompleted = true;
            CompletedAt = DateTimeOffset.UtcNow;
        CompletedBy = actorId;
            }

            public void MarkIncomplete(UserId actorId)
            {
            if (CompletedAt == null)
                throw new DomainInvalidOperationException("This item is still incomplete.");
            if (AssignedTo == null && actorId != Run.ownerId)
                throw new DomainNotAuthorizedException();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                throw new DomainNotAuthorizedException();
            IsCompleted = false;
            CompletedAt = null;
        CompletedBy = null;
            }

            public void UpdateNotes(TodoItemNotes notes, UserId actorId)
            {
            if (AssignedTo == null && actorId != Run.ownerId)
                throw new DomainNotAuthorizedException();
            if (AssignedTo != null && AssignedTo.Value != actorId)
                throw new DomainNotAuthorizedException();
            Notes = string.IsNullOrEmpty(notes.Value) ? null : notes;
        }

        public void ChangeDescription(TodoItemDescription description)
        {
            Description = description;
        }

        public void AssignTo(UserId assignedTo)
        {
        if (IsCompleted)
            throw new DomainInvalidOperationException("Couldn't asign a completed task.");
            AssignedTo = assignedTo;
        }

public void AssignToNoone()
{
        AssignedTo = null;
        }
    }
}
