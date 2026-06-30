using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.DTOs
{
    /// <summary>Represents a closed iteration in a run's history.</summary>
    public record RunIterationDto
    {
        /// <summary>The iteration identifier.</summary>
        public int Id { get; init; }
        /// <summary>When the iteration was closed.</summary>
        public DateTimeOffset? ClosedAt { get; init; }
        /// <summary>The items in this iteration at the time it was closed.</summary>
        public IReadOnlyCollection<RunItemDto> Items { get; init; }
        /// <summary>When the iteration was created.</summary>
        public DateTimeOffset CreatedAt { get; init; }
        /// <summary>When the iteration was last updated.</summary>
        public DateTimeOffset? UpdatedAt { get; init; }

        public RunIterationDto(int Id, DateTimeOffset? ClosedAt, IReadOnlyCollection<RunItemDto> Items, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt)
        {
            this.Id = Id;
            this.ClosedAt = ClosedAt;
            this.Items = Items;
            this.CreatedAt = CreatedAt;
            this.UpdatedAt = UpdatedAt;
        }
    }
}
