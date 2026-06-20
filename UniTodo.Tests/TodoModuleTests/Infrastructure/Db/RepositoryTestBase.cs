using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Infrastructure.Db;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db
{
    public abstract class RepositoryTestBase : IDisposable
    {
        private readonly SqliteConnection _connection;
        protected readonly TodoDbContext Context;

        protected RepositoryTestBase()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new TodoDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}
