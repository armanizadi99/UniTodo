using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure
{
    public class UserContext : IUserContext
    {
public UserId UserId { get; }

public UserContext(IHttpContextAccessor httpContextAccessor)
{
//UserId = userId;
        }

    }
}