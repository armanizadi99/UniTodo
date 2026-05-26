namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainException : Exception
    {
        internal DomainException(string message) : base(message) { }
    }
}
