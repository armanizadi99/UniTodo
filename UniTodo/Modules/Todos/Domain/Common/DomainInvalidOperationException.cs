namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainInvalidOperationException : DomainException
    {
        internal DomainInvalidOperationException( string message ) : base(message) { }
    }
}
