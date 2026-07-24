using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UniTodo.Modules.Todos.Infrastructure.Db;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db;

public abstract class RepositoryTestBase : IAsyncLifetime
{
    private readonly TestContainersFixture _fixture;
    private IDbContextTransaction _transaction = null!;

    protected TodoDbContext Context { get; private set; } = null!;

    protected RepositoryTestBase(TestContainersFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlServer(_fixture.ConnectionString)
            .Options;
        Context = new TodoDbContext(options);
        _transaction = await Context.Database.BeginTransactionAsync();
        await OnInitializedAsync();
    }

    protected TodoDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseSqlServer(Context.Database.GetDbConnection())
            .Options;
        var context = new TodoDbContext(options);
        context.Database.UseTransaction(_transaction.GetDbTransaction());
        return context;
    }

    protected virtual Task OnInitializedAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.RollbackAsync();
        if (Context != null)
            await Context.DisposeAsync();
    }
}