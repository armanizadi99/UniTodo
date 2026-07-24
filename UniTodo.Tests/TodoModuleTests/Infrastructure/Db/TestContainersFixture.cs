using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using UniTodo.Modules.Todos.Infrastructure.Db;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db;

public class TestContainersFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        using var context = new TodoDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("RepositoryTests")]
public class RepositoryTestCollection : ICollectionFixture<TestContainersFixture>
{
}