using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Http;
using System.Reflection;

namespace UniTodo.OpenApiEndpointFilters
{
    public class AuthorizedSecurityDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                if (apiDescription.ActionDescriptor is not ControllerActionDescriptor action)
                    continue;

                var actionAuthorize = action.MethodInfo.GetCustomAttributes<AuthorizeAttribute>(true).Any();
                var controllerAuthorize = action.ControllerTypeInfo.GetCustomAttributes<AuthorizeAttribute>(true).Any();

                var hasAllowAnonymous = action.MethodInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any()
                    || action.ControllerTypeInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any();

                if (!(actionAuthorize || controllerAuthorize) || hasAllowAnonymous)
                    continue;

                var path = "/" + apiDescription.RelativePath?.TrimEnd('/');
                if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem))
                    continue;

                var httpMethod = new HttpMethod(apiDescription.HttpMethod ?? "GET");
                if (!pathItem.Operations.TryGetValue(httpMethod, out var operation))
                    continue;

                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized - Authentication required"
                });

                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("Jwt bearer", swaggerDoc), [] }
                }
            };
            }
        }
    }
}