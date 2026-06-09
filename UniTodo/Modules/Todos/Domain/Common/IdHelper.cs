namespace UniTodo.Modules.Todos.Domain.Common
{
    internal static class IdHelper
    {
        internal static void GuardAgainstInvalid(int id, string argumentName)
        {
            if (id < 1)
                throw new ArgumentException(argumentName);
        }
    }
}
