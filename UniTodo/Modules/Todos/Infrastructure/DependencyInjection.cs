using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Infrastructure.Db;

namespace UniTodo.Modules.Todos.Infrastructure
{
    public static class DependencyInjection
    {
public static IServiceCollection AddTodosInfrastructure(
this IServiceCollection services)
{
        services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlite("Data Source=todo.db"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddHttpContextAccessor();

        return services; ;
        }

public static IServiceCollection AddTodoModule( this IServiceCollection services)
{
        services.AddTodoApplication();
        services.AddTodosInfrastructure();
        return services;
        }
    }
}
