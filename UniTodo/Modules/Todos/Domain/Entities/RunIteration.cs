using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class RunIteration : EntityBase
    {
        private readonly List<RunItem> _runItems = new List<RunItem>();

        public Run Run { get; private set; } = null!;

        public IReadOnlyCollection<RunItem> RunItems => _runItems.AsReadOnly();

        public RunIteration() { }

        public void AddItem(RunItem item)
        {
            _runItems.Add(item);
        }

        public void RemoveItem(RunItem item)
        {
            _runItems.Remove(item);
        }
    }
}
