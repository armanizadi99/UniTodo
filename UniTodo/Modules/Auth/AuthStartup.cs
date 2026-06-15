using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.Text;
using UniTodo.Modules.Auth.DB;
using UniTodo.Modules.Auth.Services;

namespace UniTodo.Modules.Auth
{
    public static class AuthStartup
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services,
        IConfigurationSection moduleConfiguration)
        {

            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlite(moduleConfiguration.GetConnectionString("sqlite"));
            });

            services.AddIdentityCore<ApplicationUser>()
                    .AddEntityFrameworkStores<AuthDbContext>()
                    .AddDefaultTokenProviders();

            var settings = new JwtSettings();
            moduleConfiguration
            .GetSection(nameof(JwtSettings))
            .Bind(settings);
            if (string.IsNullOrWhiteSpace(settings.SecretSigningKey))
                throw new InvalidOperationException("Secret signing key is required.");

            services.AddSingleton(settings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudience = settings.Audience,
                    ValidateAudience = true,
                    ValidIssuer = settings.Issuer,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretSigningKey))
                };
            });

            services.AddSingleton<JwtTokenCreater>();

            return services;
        }
    }
}
