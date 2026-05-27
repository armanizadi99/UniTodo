using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddTodoApplication(
        this IServiceCollection services)
        {
            services.AddScoped<ITodoListTemplateService, TodoListTemplateService>();
            services.AddScoped<ITodoListRunService, TodoListRunService>();
            return services;
        }
    }
}