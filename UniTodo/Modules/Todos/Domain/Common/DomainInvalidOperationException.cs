namespace UniTodo.Modules.Todos.Domain.Common
{
    public class DomainInvalidOperationException : DomainException
    {
        public DomainInvalidOperationException( string message ) : base(message) { }
    }
}
