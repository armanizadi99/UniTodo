using UniTodo.Modules.Todos.Api.Controllers;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Infrastructure;

namespace UniTodo.Modules.Todos.ModuleStartup
{
    public static class TodoModuleStartup
    {
        public static IServiceCollection AddTodoModule(
        this IServiceCollection services,
        IConfigurationSection moduleConfiguration)
        {
            services.AddTodoApplication();
            services.AddTodoInfrastructure(moduleConfiguration);

            return services;
        }

        public static IEndpointRouteBuilder MapTodoEndpoints(
        this IEndpointRouteBuilder endpointRouteBuilder)
        {
            return endpointRouteBuilder;
        }
    }
}
