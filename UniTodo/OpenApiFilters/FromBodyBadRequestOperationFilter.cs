using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UniTodo.OpenApiEndpointFilters
{
    public class FromBodyBadRequestOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasBodyParam = context.ApiDescription.ParameterDescriptions
                .Any(p => p.Source == BindingSource.Body);

            if (!hasBodyParam)
                return;

            if (operation.Responses?.ContainsKey("400") == true)
                return;

            var problemSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);

            operation.Responses["400"] = new OpenApiResponse
            {
                Description = "Bad Request - Model binding failed",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = problemSchema
                    }
                }
            };
        }
    }
}
