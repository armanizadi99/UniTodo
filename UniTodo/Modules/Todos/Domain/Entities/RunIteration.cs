using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class RunIteration : EntityBase
    {
        private readonly List<RunItem> _runItems = new List<RunItem>();

        public DateTimeOffset? ClosedAt { get; private set; }

        public Run Run { get; private set; } = null!;

        public IReadOnlyCollection<RunItem> RunItems => _runItems.AsReadOnly();

        public RunIteration() { }

        public Result AddItem(RunItem item)
        {
            _runItems.Add(item);
        return Result.Success();
        }

        public Result  RemoveItem(RunItem item)
        {
            _runItems.Remove(item);
        return Result.Success();
        }

public Result Close()
{
ClosedAt = DateTimeOffset.UtcNow;
        return Result.Success();
        }
    }
}
