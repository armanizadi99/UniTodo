using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Interfaces
{
    internal interface IUserContext
    {
        UserId UserId { get; }
    }
}
