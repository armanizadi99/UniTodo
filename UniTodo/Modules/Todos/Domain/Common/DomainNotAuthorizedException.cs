namespace UniTodo.Modules.Todos.Domain.Common
{
    public class DomainNotAuthorizedException : DomainException
    {
        public DomainNotAuthorizedException( string message = "") : base(message) { }
    }
}
