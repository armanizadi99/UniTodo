using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure
{
    internal class UserContext : IUserContext
    {
UserId IUserContext.UserId { get; }

        public UserContext( IHttpContextAccessor httpContextAccessor)
{
//UserId = userId;
        }

    }
}