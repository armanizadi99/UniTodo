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
this IServiceCollection services, 
IConfigurationSection moduleConfiguration)
{
        services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlite(moduleConfiguration.GetConnectionString("sqlite")));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped(typeof(IRepositoryWithTypedId<,>), typeof(RepositoryWithTypedId<,>));

//this is a singleton service that other services also might require, so I have to later move it into the host. Registering it multiple times doesn't break anything, but well, it seams a bit dirty.
        services.AddHttpContextAccessor();

        return services;
        }

public static IServiceCollection AddTodoModule( this IServiceCollection services, 
IConfigurationSection moduleConfiguration)
{
        services.AddTodoApplication();
        services.AddTodosInfrastructure(moduleConfiguration);
        return services;
        }
    }
}
