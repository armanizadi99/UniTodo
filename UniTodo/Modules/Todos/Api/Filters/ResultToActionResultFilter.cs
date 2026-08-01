using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using UniTodo.Modules.Todos.Api.Attributes;
using UniTodo.Modules.Todos.Domain.Common;

namespace UniTodo.Modules.Todos.Api.Filters
{
    public sealed class ResultToActionResultFilter : IAsyncActionFilter
    {
        private static readonly Type ResultTypeDefinition = typeof(Result<>);

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Result is not ObjectResult objectResult || objectResult.Value is null)
                return;

            var value = objectResult.Value;

            if (value is Result result)
            {
                executedContext.Result = result.IsSuccess
                    ? new NoContentResult()
                    : ToHttpResult(result.Error);
                return;
            }

            var valueType = value.GetType();
            if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != ResultTypeDefinition)
                return;

            if (!(bool)valueType.GetProperty("IsSuccess")!.GetValue(value)!)
            {
                executedContext.Result = ToHttpResult((DomainError)valueType.GetProperty("Error")!.GetValue(value)!);
                return;
            }

            var innerValue = valueType.GetProperty("Value")!.GetValue(value)!;
            var createdAttribute = executedContext.ActionDescriptor.EndpointMetadata
                .OfType<CreatedAtRouteResultAttribute>()
                .FirstOrDefault();

            if (createdAttribute is null)
            {
                executedContext.Result = new OkObjectResult(innerValue);
                return;
            }

            var id = innerValue.GetType().GetProperty("Id")?.GetValue(innerValue);
            executedContext.Result = new CreatedAtRouteResult(
                createdAttribute.RouteName,
                new RouteValueDictionary { [createdAttribute.RouteValueName] = id },
                innerValue);
        }

        private static IActionResult ToHttpResult(DomainError error)
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
