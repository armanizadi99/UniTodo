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
    public class RunRepositoryTests : RepositoryTestBase
    {
        private readonly IRunRepository _repository;

        public RunRepositoryTests()
        {
            _repository = new RunRepository(Context);
        }

        [Fact]
        public async Task GetRunByIdAsync_ShouldReturnRun_WhenRunExists()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new Run("Test Run", ResetPolicy.None, false, ownerId);
            await Context.runs.AddAsync(run);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRunByIdAsync(run.Id, false, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Run");
        }

        [Fact]
        public async Task GetRunByIdAsync_ShouldReturnRunIncludingItems_WhenIncludeItemsIsTrue()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new Run("Test Run", ResetPolicy.None, false, ownerId);
            run.AddRunItem(new RunItem(new TodoItemDescription("Item 1")), ownerId);
            await Context.runs.AddAsync(run);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRunByIdAsync(run.Id, true, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.CurrentIteration.RunItems.Should().HaveCount(1);
            result.CurrentIteration.RunItems.First().Description.Value.Should().Be("Item 1");
        }

        [Fact]
        public async Task GetRunByIdAsync_ShouldOnlyLoadCurrentIterationItems_NotHistoricalOnes()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new Run("Test Run", ResetPolicy.None, false, ownerId);
            run.AddRunItem(new RunItem(new TodoItemDescription("Original Item")), ownerId);
            await Context.runs.AddAsync(run);
            await Context.SaveChangesAsync();

            run.Reset(ownerId);
            await Context.SaveChangesAsync();

            // Act
            using var actContext = CreateNewContext();
            var repository = new RunRepository(actContext);
            var result = await ((IRunRepository)repository).GetRunByIdAsync(run.Id, true, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Iterations.Should().HaveCount(1);
            result.CurrentIteration.RunItems.Should().ContainSingle();
            result.CurrentIteration.RunItems.First().Description.Value.Should().Be("Original Item");
            result.CurrentIteration.RunItems.First().IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserActiveRunsAsync_ShouldReturnActiveRunsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = new UserId(userId);

            var activeRun = new Run("Active Run", ResetPolicy.None, false, ownerId);

            var closedRun = new Run("Closed Run", ResetPolicy.None, false, ownerId);
            closedRun.Close(ownerId);

            var otherUserRun = new Run("Other Run", ResetPolicy.None, false, new UserId(Guid.NewGuid()));

            await Context.runs.AddRangeAsync(activeRun, closedRun, otherUserRun);
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
            var runDue = new Run("Due Run", ResetPolicy.Daily, false, ownerId);
            // Manually set ResetsAt to the past to simulate time passing
            var resetsAtField = typeof(Run).GetProperty(nameof(Run.ResetsAt));
            resetsAtField!.SetValue(runDue, DateTimeOffset.UtcNow.AddHours(-1));

            // Run not yet due
            var runNotDue = new Run("Not Due Run", ResetPolicy.Daily, false, ownerId);
            resetsAtField.SetValue(runNotDue, DateTimeOffset.UtcNow.AddHours(23));

            // Run with no reset policy
            var runNoPolicy = new Run("No Policy Run", ResetPolicy.None, false, ownerId);

            await Context.runs.AddRangeAsync(runDue, runNotDue, runNoPolicy);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRunsDueForResetAsync(CancellationToken.None);

            // Assert
            result.Should().Contain(r => r.Name == "Due Run");
            result.Should().NotContain(r => r.Name == "Not Due Run");
            result.Should().NotContain(r => r.Name == "No Policy Run");
        }

        [Fact]
        public async Task GetRunsDueForResetAsync_ShouldLoadCurrentIterationItemsForCloning()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var runDue = new Run("Due Run", ResetPolicy.Daily, false, ownerId);
            runDue.AddRunItem(new RunItem(new TodoItemDescription("Item 1")), ownerId);
            var resetsAtField = typeof(Run).GetProperty(nameof(Run.ResetsAt));
            resetsAtField!.SetValue(runDue, DateTimeOffset.UtcNow.AddHours(-1));

            await Context.runs.AddAsync(runDue);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetRunsDueForResetAsync(CancellationToken.None);

            // Assert
            var dueRun = result.Should().ContainSingle().Subject;
            dueRun.CurrentIteration.RunItems.Should().ContainSingle()
                .Which.Description.Value.Should().Be("Item 1");
        }

        [Fact]
        public async Task GetRunByIdAsync_WithItemId_ShouldReturnRunAndSpecificItem()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new Run("Test Run", ResetPolicy.None, false, ownerId);
            run.AddRunItem(new RunItem(new TodoItemDescription("Item 1")), ownerId);
            run.AddRunItem(new RunItem(new TodoItemDescription("Item 2")), ownerId);
            await Context.runs.AddAsync(run);
            await Context.SaveChangesAsync();

            var itemId = run.CurrentIteration.RunItems.First().Id;

            // Use fresh context to verify eager loading filtered items
            using (var actContext = CreateNewContext())
            {
                var repository = new RunRepository(actContext);

                // Act
                var result = await ((IRunRepository)repository).GetRunByIdAsync(run.Id, itemId, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.CurrentIteration.RunItems.Should().HaveCount(1);
                result.CurrentIteration.RunItems.First().Id.Should().Be(itemId);
            }
        }

        [Fact]
        public async Task RunMember_FK_ShouldEnforceUniquenessAgainstRunId()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var run = new Run("Test Run", ResetPolicy.None, true, ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, ownerId);
            await Context.runs.AddAsync(run);
            await Context.SaveChangesAsync();

            // Act: attempt to insert a duplicate (UserId, RunId) pair directly, bypassing domain validation,
            // to prove the DB-level composite key still rejects it under real SQLite enforcement.
            using var actContext = CreateNewContext();
            var duplicateMember = new RunMember(memberId);
            actContext.runMembers.Add(duplicateMember);
            actContext.Entry(duplicateMember).Property("RunId").CurrentValue = run.Id;

            // Assert
            var act = async () => await actContext.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
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
