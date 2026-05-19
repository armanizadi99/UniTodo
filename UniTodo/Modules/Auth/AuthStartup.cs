using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Auth.DB;

namespace UniTodo.Modules.Auth
{
    public static class AuthStartup
    {
public static IServiceCollection AddAuthModule(this IServiceCollection services,
IConfigurationSection moduleConfiguration) {
        
        services.AddDbContext<AuthDbContext>(options =>
        {
        options.UseSqlite(moduleConfiguration.GetConnectionString("sqlite"));
        });

        services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

        return services;
        }
    }
}
