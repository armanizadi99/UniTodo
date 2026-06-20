using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    public record TodoListRunDto
    {
        /// <summary>The database identifier of this run.</summary>
        public int Id { get; init; }
        /// <summary>The public GUID identifying this run.</summary>
        public Guid RunId { get; init; }
        /// <summary>The display name of the run.</summary>
        public string Name { get; init; }
        /// <summary>The reset policy governing item reset behavior.</summary>
        public ResetPolicy ResetPolicy { get; init; }
        /// <summary>The user ID of the owner.</summary>
        public Guid OwnerId { get; init; }
        /// <summary>The current status of the run.</summary>
        public TodoListRunStatus Status { get; init; }
        /// <summary>Whether the run is shared with other members.</summary>
        public bool IsShared { get; init; }
        /// <summary>When the run was closed, if applicable.</summary>
        public DateTimeOffset? ClosedAt { get; init; }
        /// <summary>When the run was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the run was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public TodoListRunDto(int Id, Guid RunId, string Name, ResetPolicy ResetPolicy, Guid OwnerId, TodoListRunStatus Status, bool IsShared, DateTimeOffset? ClosedAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.RunId = RunId;
            this.Name = Name;
            this.ResetPolicy = ResetPolicy;
            this.OwnerId = OwnerId;
            this.Status = Status;
            this.IsShared = IsShared;
            this.ClosedAt = ClosedAt;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
