using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Application.Common
{
    public class DomainEntityNotFoundException : DomainException
    {
        public string EntityName { get; private set; }
public int Id { get; private set; }

        public DomainEntityNotFoundException( string entityName, int id     ) : base("")
        {
        EntityName = entityName;
        Id = id;
        }
    }
}
