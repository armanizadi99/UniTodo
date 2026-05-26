namespace UniTodo.Modules.Todos.Domain.Common
{
    internal class DomainArgumentException : DomainException
    {
        internal string ArgumentName { get; private set; }

        internal DomainArgumentException(string argumentName, string message = "") : base(message)
        {
            ArgumentName = argumentName;
        }
    }
}
