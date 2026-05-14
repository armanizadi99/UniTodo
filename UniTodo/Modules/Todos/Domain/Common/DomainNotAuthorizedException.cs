namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainNotAuthorizedException : DomainException
    {
        internal DomainNotAuthorizedException( string message = "") : base(message) { }
    }
}
