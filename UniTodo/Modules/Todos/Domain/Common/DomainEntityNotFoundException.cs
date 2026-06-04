namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainEntityNotFoundException : DomainException
    {
        internal string EntityName { get; private set; }
        internal int Id { get; private set; }

        internal DomainEntityNotFoundException(string entityName, int id) : base($"Entity '{entityName}' with id '{id}' was not found.")
        {
            EntityName = entityName;
            Id = id;
        }
    }
}
