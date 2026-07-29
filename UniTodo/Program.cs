using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using UniTodo.Modules.Auth;
using UniTodo.Modules.Todos.Infrastructure;
using UniTodo.Modules.Todos.ModuleStartup;
using UniTodo.OpenApiEndpointFilters;

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console()
.CreateLogger();
try
{
    Log.Information("Starting application");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, loggerConfig) =>
    {
        var seqUrl = context.Configuration["SEQ_URL"] ?? "http://localhost:5341";
        var seqApiKey = context.Configuration["SEQ_API_KEY"];
        if (string.IsNullOrEmpty(seqApiKey))
            throw new InvalidOperationException("SEQ_API_KEY is required. Set it via user secrets, env var, or appsettings.json.");

        Log.Information("Configuring Seq sink: {SeqUrl}", seqUrl);

        loggerConfig
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.Seq(serverUrl: seqUrl, apiKey: seqApiKey);
    });
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

    builder.Services.AddProblemDetails();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UniTodo API",
            Version = "1.0",
            Description = "API for managing shared todo list templates, runs, and items. Supports user authentication via JWT, collaborative runs with member permissions, iteration-based reset policies, and scheduling settings."
        });

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

        options.DocumentFilter<AuthorizedSecurityDocumentFilter>();
        //options.OperationFilter<FromBodyBadRequestOperationFilter>();
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

    // Automatically apply migrations with retry
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var maxRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
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

                break;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Log.Warning(ex, "Database migration attempt {Attempt} failed, retrying in {Delay}...", attempt, retryDelay);
                Thread.Sleep(retryDelay);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating the database after {MaxRetries} attempts.", maxRetries);
                throw;
            }
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