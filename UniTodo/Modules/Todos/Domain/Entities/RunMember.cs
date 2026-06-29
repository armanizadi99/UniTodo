using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Domain.Entities
{
    public class RunMember : EntityBase
    {
        public UserId UserId { get; private set; }
        public int RunId { get; private set; }
        public Run Run { get; private set; } = null!;

        private RunMember() { }

        public RunMember(UserId userId)
        {
            UserId = userId;
        }
    }
}
