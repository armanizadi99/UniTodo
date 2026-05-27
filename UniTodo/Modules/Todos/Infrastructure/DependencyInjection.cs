using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Infrastructure.Db;
using UniTodo.Modules.Todos.Infrastructure.Db.Repositories;

namespace UniTodo.Modules.Todos.Infrastructure
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection AddTodoInfrastructure(
        this IServiceCollection services,
        IConfigurationSection moduleConfiguration)
        {
            services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlite(moduleConfiguration.GetConnectionString("sqlite")));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped(typeof(IRepositoryWithTypedId<,>), typeof(RepositoryWithTypedId<,>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITodoListTemplateRepository, TodoListTemplateRepository>();
            services.AddScoped<ITodoListRunRepository, TodoListRunRepository>();

            //this is a singleton service that other services also might require, so I have to later move it into the host. Registering it multiple times doesn't break anything, but well, it seams a bit dirty.
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
