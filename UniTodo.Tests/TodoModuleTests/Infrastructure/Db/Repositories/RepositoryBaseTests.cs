using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Infrastructure.Db.Repositories;
using UniTodo.Modules.Todos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db.Repositories
{
    public class RepositoryBaseTests : RepositoryTestBase
    {
        private readonly IRepository<TodoListTemplate> _repository;

        public RepositoryBaseTests()
        {
            _repository = new Repository<TodoListTemplate>(Context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntityToDatabase()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "New Template", ResetPolicy.None);

            // Act
            await _repository.AddAsync(template);
            await Context.SaveChangesAsync();

            // Assert
            var result = await Context.todoLists.FirstOrDefaultAsync(t => t.Name == "New Template");
            result.Should().NotBeNull();
            result!.OwnerId.Should().Be(ownerId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenItExists()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "Existing Template", ResetPolicy.None);
            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(template.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Existing Template");
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnFilteredEntities()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template1 = new TodoListTemplate(ownerId, "A Template", ResetPolicy.None);
            var template2 = new TodoListTemplate(ownerId, "B Template", ResetPolicy.None);
            await Context.todoLists.AddRangeAsync(template1, template2);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetListAsync(t => t.Name.StartsWith("A"), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("A Template");
        }

        [Fact]
        public async Task Update_ShouldModifyExistingEntity()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "Old Name", ResetPolicy.None);
            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Act
            template.Archive(ownerId);
            _repository.Update(template);
            await Context.SaveChangesAsync();

            // Assert
            var result = await Context.todoLists.FindAsync(template.Id);
            result!.Status.Should().Be(TodoListStatus.Archived);
        }

        [Fact]
        public async Task Remove_ShouldDeleteEntity()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "To Be Deleted", ResetPolicy.None);
            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Act
            _repository.Remove(template);
            await Context.SaveChangesAsync();

            // Assert
            var result = await Context.todoLists.FindAsync(template.Id);
            result.Should().BeNull();
        }
    }
}
