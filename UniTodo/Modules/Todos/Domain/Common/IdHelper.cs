namespace UniTodo.Modules.Todos.Domain.Common
{
    public static class IdHelper
    {
        public static void GuardAgainstInvalid( int id, string argumentName)
{
        if (id < 1)
            throw new DomainArgumentException(argumentName);
        }
    }
}
