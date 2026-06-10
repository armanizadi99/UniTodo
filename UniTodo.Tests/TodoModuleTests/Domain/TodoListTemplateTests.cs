using FluentAssertions;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Domain
{
    public class TodoListTemplateTests
    {
        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var name = "Test List";
            var resetPolicy = ResetPolicy.Daily;

            // Act
            var todoList = new TodoListTemplate(ownerId, name, resetPolicy);

            // Assert
            todoList.OwnerId.Should().Be(ownerId);
            todoList.Name.Should().Be(name);
            todoList.ResetPolicy.Should().Be(resetPolicy);
            todoList.Status.Should().Be(TodoListStatus.Active);
        }

        [Fact]
        public void Archive_ShouldChangeStatusToArchivedAndReturnSuccess()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(ownerId, "Test List", ResetPolicy.Daily);

            // Act
            var result = todoList.Archive(ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Archived);
        }

        [Fact]
        public void Archive_ShouldReturnNotAuthorizedError_WhenActorIsNotOwner()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(ownerId, "Test List", ResetPolicy.Daily);

            // Act
            var result = todoList.Archive(otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public void MakeActive_ShouldChangeStatusToActiveAndReturnSuccess()
        {
            // Arrange
            var ownerId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(ownerId, "Test List", ResetPolicy.Daily);
            todoList.Archive(ownerId);

            // Act
            var result = todoList.MakeActive(ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Active);
        }
    }
}
