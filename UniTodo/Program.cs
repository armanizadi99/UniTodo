using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Formatting.Json;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using UniTodo.Modules.Auth;
using UniTodo.Modules.Todos.Infrastructure;
using UniTodo.Modules.Todos.ModuleStartup;

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console()
.WriteTo.File(new JsonFormatter(), "data/logs/log-.json", rollingInterval: RollingInterval.Day)
.CreateLogger();
try
{
    Log.Information("Starting application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    // Add services to the container.
    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
    .Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    });
    builder.Services.AddHttpLogging();
    builder.Services.AddTodoModule(builder.Configuration.GetSection("TodoModule"));
    builder.Services.AddAuthModule(builder.Configuration.GetSection("AuthModule"));

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "UniTodo API", Version = "1.0" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.AddSecurityDefinition("Jwt bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1Ni...\""
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
{
new OpenApiSecuritySchemeReference("Jwt bearer", document),
new List<string>()
    }
    });

    });
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging();
    app.UseHttpLogging();

    // Automatically apply migrations
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var authContext = services.GetRequiredService<UniTodo.Modules.Auth.DB.AuthDbContext>();
            if (authContext.Database.GetPendingMigrations().Any())
            {
                Log.Information("Applying Auth migrations...");
                authContext.Database.Migrate();
            }

            var todoContext = services.GetRequiredService<UniTodo.Modules.Todos.Infrastructure.Db.TodoDbContext>();
            if (todoContext.Database.GetPendingMigrations().Any())
            {
                Log.Information("Applying Todo migrations...");
                todoContext.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database.");
            throw; // Fail fast if we can't migrate
        }
    }

    app.MapControllers();
    app.MapTodoEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}