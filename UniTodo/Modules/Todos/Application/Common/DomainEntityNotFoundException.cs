using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Common
{
    internal class DomainEntityNotFoundException : DomainException
    {
        internal string EntityName { get; private set; }
        internal int Id { get; private set; }

        internal DomainEntityNotFoundException(string entityName, int id) : base("")
        {
            EntityName = entityName;
            Id = id;
        }
    }
}
