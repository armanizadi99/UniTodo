namespace UniTodo.Modules.Todos.Domain.Common
{
    public class DomainArgumentException : DomainException
    {
public string ArgumentName { get; private set; }

public DomainArgumentException(string argumentName, string message = "") : base(message)
{
        ArgumentName = argumentName;
        }
    }
}
