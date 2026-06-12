using UniTodo.Modules.Todos.Application.BackgroundServices;
using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddTodoApplication(
        this IServiceCollection services)
        {
            services.AddScoped<TodoListTemplateService>();
            services.AddScoped<TodoListRunService>();
            services.AddHostedService<ResetPolicyJob>();

            return services;
        }
    }
}