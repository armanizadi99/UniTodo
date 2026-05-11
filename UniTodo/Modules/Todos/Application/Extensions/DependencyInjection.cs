using UniTodo.Modules.Todos.Application.Services;

namespace UniTodo.Modules.Todos.Application.Extensions
{
    public static class DependencyInjection
    {
public static IServiceCollection AddTodoApplication(
this IServiceCollection services)
{
        services.AddScoped<ITodoListTemplateService, TodoListTemplateService>();
        services.AddScoped<ITodoItemTemplateService, TodoItemTemplateService>();
        return services;
        }
    }
}