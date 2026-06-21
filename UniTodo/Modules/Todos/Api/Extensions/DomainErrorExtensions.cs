using Microsoft.AspNetCore.Mvc;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Api.Extensions
{
    public static class DomainErrorExtensions
    {
        public static IActionResult ToActionResult(this DomainError error)
        {
            return error.Code switch
            {
                DomainErrorCodes.EntityNotFound => new NotFoundObjectResult(new ProblemDetails
                {
                    Detail = error.Message,
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Type = "https://httpstatuses.com/404"
                }),
                DomainErrorCodes.NotAuthorized => new ForbidResult(),
                DomainErrorCodes.InvalidOperation => new BadRequestObjectResult(new ProblemDetails
                {
                    Detail = error.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Type = "https://httpstatuses.com/400"
                }),
                DomainErrorCodes.DuplicateEntities => new ConflictObjectResult(new ProblemDetails
                {
                    Detail = error.Message,
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Type = "https://httpstatuses.com/409"
                }),
                _ => new ObjectResult(new ProblemDetails
                {
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Type = "https://httpstatuses.com/500"
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
            };
        }
    }
}
