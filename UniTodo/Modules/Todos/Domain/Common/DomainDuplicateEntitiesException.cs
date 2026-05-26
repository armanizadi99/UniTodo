namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainDuplicateEntitiesException : DomainException
    {
        internal DomainDuplicateEntitiesException(string message) : base(message) { }
    }
}
