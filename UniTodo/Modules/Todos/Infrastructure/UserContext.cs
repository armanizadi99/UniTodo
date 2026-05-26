using System.Security.Claims;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Infrastructure
{
    internal class UserContext : IUserContext
    {
        private readonly UserId _userId;
        UserId IUserContext.UserId
        {
            get
            {
                return _userId;
            }
        }

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            var subClaim = httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (subClaim is not null && Guid.TryParse(subClaim, out var userId))
            {
                _userId = new UserId(userId);
            }
            else
                throw new DomainInvalidOperationException("User is Either not authenticated, or something went wrong in the api.");
        }
    }
}