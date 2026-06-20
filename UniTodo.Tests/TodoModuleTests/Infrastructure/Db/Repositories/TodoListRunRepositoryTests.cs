using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Infrastructure.Db.Repositories;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db.Repositories
{
    public class TodoListRunRepositoryTests : RepositoryTestBase
    {
        private readonly ITodoListRunRepository _repository;

        public TodoListRunRepositoryTests()
        {
            _repository = new TodoListRunRepository(Context);
        }

        [Fact]
        public async Task GetTodoListRunByIdAsync_ShouldReturnRun_WhenRunExists()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new TodoListRun("Test Run", ResetPolicy.None, false, ownerId);
            await Context.todoListRuns.AddAsync(run);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTodoListRunByIdAsync(run.Id, false, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Run");
        }

        [Fact]
        public async Task GetTodoListRunByIdAsync_ShouldReturnRunIncludingItems_WhenIncludeItemsIsTrue()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new TodoListRun("Test Run", ResetPolicy.None, false, ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("Item 1")), ownerId);
            await Context.todoListRuns.AddAsync(run);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTodoListRunByIdAsync(run.Id, true, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.TodoItems.Should().HaveCount(1);
            result.TodoItems.First().Description.Value.Should().Be("Item 1");
        }

        [Fact]
        public async Task GetUserActiveRunsAsync_ShouldReturnActiveRunsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = new UserId(userId);
            
            var activeRun = new TodoListRun("Active Run", ResetPolicy.None, false, ownerId);
            
            var closedRun = new TodoListRun("Closed Run", ResetPolicy.None, false, ownerId);
            closedRun.Close(ownerId);

            var otherUserRun = new TodoListRun("Other Run", ResetPolicy.None, false, new UserId(Guid.NewGuid()));

            await Context.todoListRuns.AddRangeAsync(activeRun, closedRun, otherUserRun);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserActiveRunsAsync(userId, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(r => r.Name == "Active Run");
        }

        [Fact]
        public async Task GetRunsDueForResetAsync_ShouldReturnRunsThatNeedReset()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            
            // Run due for reset (Daily, started yesterday)
            var runDue = new TodoListRun("Due Run", ResetPolicy.Daily, false, ownerId);
            // Manually set ResetsAt to the past to simulate time passing
            var resetsAtField = typeof(TodoListRun).GetProperty(nameof(TodoListRun.ResetsAt));
            resetsAtField!.SetValue(runDue, DateTimeOffset.UtcNow.AddHours(-1));
            
            // Run not yet due
            var runNotDue = new TodoListRun("Not Due Run", ResetPolicy.Daily, false, ownerId);
            resetsAtField.SetValue(runNotDue, DateTimeOffset.UtcNow.AddHours(23));

            // Run with no reset policy
            var runNoPolicy = new TodoListRun("No Policy Run", ResetPolicy.None, false, ownerId);

            await Context.todoListRuns.AddRangeAsync(runDue, runNotDue, runNoPolicy);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRunsDueForResetAsync(CancellationToken.None);

            // Assert
            result.Should().Contain(r => r.Name == "Due Run");
            result.Should().NotContain(r => r.Name == "Not Due Run");
            result.Should().NotContain(r => r.Name == "No Policy Run");
        }
        [Fact]
        public async Task GetTodoListRunByIdAsync_WithItemId_ShouldReturnRunAndSpecificItem()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new TodoListRun("Test Run", ResetPolicy.None, false, ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("Item 1")), ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("Item 2")), ownerId);
            await Context.todoListRuns.AddAsync(run);
            await Context.SaveChangesAsync();

            var itemId = run.TodoItems.First().Id;

            // Use fresh context to verify eager loading filtered items
            using (var actContext = CreateNewContext())
            {
                var repository = new TodoListRunRepository(actContext);

                // Act
                var result = await ((ITodoListRunRepository)repository).GetTodoListRunByIdAsync(run.Id, itemId, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.TodoItems.Should().HaveCount(1);
                result.TodoItems.First().Id.Should().Be(itemId);
            }
        }

        private TodoDbContext CreateNewContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseSqlite(Context.Database.GetDbConnection())
                .Options;
            return new TodoDbContext(options);
        }
    }
}
