using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Infrastructure.Db.Repositories;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Todos.Domain.Enums;

namespace UniTodo.Tests.TodoModuleTests.Infrastructure.Db.Repositories
{
    public class TodoListTemplateRepositoryTests : RepositoryTestBase
    {
        private readonly ITodoListTemplateRepository _repository;

        public TodoListTemplateRepositoryTests()
        {
            _repository = new TodoListTemplateRepository(Context);
        }

        [Fact]
        public async Task IsNameDuplicateAsync_ShouldReturnTrue_WhenNameExists()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "Existing Template", ResetPolicy.None);
            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.IsNameDuplicateAsync("Existing Template", CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsNameDuplicateAsync_ShouldReturnFalse_WhenNameDoesNotExist()
        {
            // Arrange
            // No templates added

            // Act
            var result = await _repository.IsNameDuplicateAsync("Non-existent Template", CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserTodoListTemplatesAsync_ShouldReturnUserTemplates_WhenUserHasTemplates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ownerId = new UserId(userId);
            var otherUserId = new UserId(Guid.NewGuid());
            var template1 = new TodoListTemplate(ownerId, "Template 1", ResetPolicy.None);
            var template2 = new TodoListTemplate(ownerId, "Template 2", ResetPolicy.None);
            var template3 = new TodoListTemplate(otherUserId, "Template 3", ResetPolicy.None);

            await Context.todoLists.AddRangeAsync(template1, template2, template3);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserTodoListTemplatesAsync(userId, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Name == "Template 1");
            result.Should().Contain(t => t.Name == "Template 2");
            result.Should().NotContain(t => t.Name == "Template 3");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnTemplateWithItems_WhenIncludeItemsIsTrue()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "Template", ResetPolicy.None);
            template.AddTodoItemTemplate(new TodoItemTemplate(0, new TodoItemDescription("Item 1")), ownerId);
            template.AddTodoItemTemplate(new TodoItemTemplate(0, new TodoItemDescription("Item 2")), ownerId);

            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTodoListTemplateByIdAsync(template.Id, true, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.TodoItemTemplates.Should().HaveCount(2);
            result.TodoItemTemplates.Should().Contain(i => i.Description.Value == "Item 1");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnTemplateWithoutItems_WhenIncludeItemsIsFalse()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(ownerId, "Template", ResetPolicy.None);
            template.AddTodoItemTemplate(new TodoItemTemplate(0, new TodoItemDescription("Item 1")), ownerId);

            await Context.todoLists.AddAsync(template);
            await Context.SaveChangesAsync();

            // Use a separate context instance for Act to verify Lazy Loading / Eager Loading behavior
            using (var actContext = CreateNewContext())
            {
                ITodoListTemplateRepository repository = new TodoListTemplateRepository(actContext);
                
                // Act
                var result = await repository.GetTodoListTemplateByIdAsync(template.Id, false, CancellationToken.None);

                // Assert
                result.Should().NotBeNull();
                result!.TodoItemTemplates.Should().BeEmpty();
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
