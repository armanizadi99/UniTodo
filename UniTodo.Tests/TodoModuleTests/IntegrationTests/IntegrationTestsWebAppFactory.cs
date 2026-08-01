using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using UniTodo.Modules.Auth.DB;
using UniTodo.Modules.Todos.Infrastructure.Db;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class IntegrationTestsWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
.WithImage("mcr.microsoft.com/mssql/server:2022-latest")
.Build();

        private DbConnection _dbConnection = null!;
        private Respawner _respawner = null!;

        public static readonly string DummyJwtSecretKey = "ThisIsADummySecretKeyThatIsAtLeast32CharactersLong!";

        public async Task InitializeAsync()
        {
        await _dbContainer.StartAsync();

        _dbConnection = new SqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
DbAdapter = DbAdapter.SqlServer,
SchemasToInclude = new[] { "dbo" }
        });
        }

public async Task ResetDatabaseAsync()
{
        await _respawner.ResetAsync(_dbConnection);
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
        await _dbConnection.DisposeAsync();
        await _dbContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost( IWebHostBuilder builder )
        {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(( context, configBuilder ) =>
{
var testConfig = new Dictionary<string, string?>
        {
            { "AuthModule:JwtSettings:SecretSigningKey", DummyJwtSecretKey },
            { "SEQ_API_KEY", "dummy-test-api-key" },
            { "Serilog:WriteTo:0:Name", "Console" }
        };

configBuilder.AddInMemoryCollection(testConfig);
});

        builder.ConfigureTestServices(services =>
        {
        var todosDbContextDescriptor = services.SingleOrDefault(
        d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));

        if (todosDbContextDescriptor != null)
            services.Remove(todosDbContextDescriptor);

        services.AddDbContext<TodoDbContext>(options =>
        options.UseSqlServer(_dbContainer.GetConnectionString()));
        
        var sp = services.BuildServiceProvider();
using var scope = sp.CreateScope();
var db = scope.ServiceProvider.GetService<TodoDbContext>();
        db!.Database.Migrate();
        });
        }
    }

    [CollectionDefinition("SharedIntegrationCollection")]
    public class SharedIntegrationCollection : ICollectionFixture<IntegrationTestsWebAppFactory>
    { }
}
