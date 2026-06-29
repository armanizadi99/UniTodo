using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;
using FluentAssertions;

namespace UniTodo.Tests.TodoModuleTests.Domain
{
    public class RunItemTests
    {
        private readonly UserId _actorId = new UserId(Guid.NewGuid());

        #region MarkComplete/MarkIncomplete Tests
        [Fact]
        public void MarkComplete_WhenIncomplete_ShouldMarkCompleteAndRecordActorAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));

            // Act
            var result = item.MarkComplete(_actorId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            item.CompletedBy.Should().Be(_actorId);
        }

        [Fact]
        public void MarkComplete_WhenAlreadyComplete_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            item.MarkComplete(_actorId);

            // Act
            var result = item.MarkComplete(_actorId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This item is already marked complete.");
        }

        [Fact]
        public void MarkIncomplete_WhenComplete_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            item.MarkComplete(_actorId);

            // Act
            var result = item.MarkIncomplete();

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
        }

        [Fact]
        public void MarkIncomplete_WhenAlreadyIncomplete_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));

            // Act
            var result = item.MarkIncomplete();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This item is still incomplete.");
        }
        #endregion

        #region UpdateNotes Tests
        [Fact]
        public void UpdateNotes_WithValue_ShouldSetNotesAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            var notes = new TodoItemNotes("Some notes");

            // Act
            var result = item.UpdateNotes(notes);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WithEmptyValue_ShouldClearNotes()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            item.UpdateNotes(new TodoItemNotes("Some notes"));

            // Act
            var result = item.UpdateNotes(new TodoItemNotes(""));

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().BeNull();
        }
        #endregion

        #region ChangeDescription Tests
        [Fact]
        public void ChangeDescription_ShouldUpdateDescriptionAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Old"));
            var newDescription = new TodoItemDescription("New");

            // Act
            var result = item.ChangeDescription(newDescription);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Description.Should().Be(newDescription);
        }
        #endregion

        #region AssignTo/AssignToNoone Tests
        [Fact]
        public void AssignTo_WhenIncomplete_ShouldAssignAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            var assignee = new UserId(Guid.NewGuid());

            // Act
            var result = item.AssignTo(assignee);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().Be(assignee);
        }

        [Fact]
        public void AssignTo_WhenCompleted_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            item.MarkComplete(_actorId);
            var assignee = new UserId(Guid.NewGuid());

            // Act
            var result = item.AssignTo(assignee);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Couldn't asign a completed task.");
        }

        [Fact]
        public void AssignToNoone_ShouldClearAssignmentAndReturnSuccess()
        {
            // Arrange
            var item = new RunItem(new TodoItemDescription("Test Item"));
            item.AssignTo(new UserId(Guid.NewGuid()));

            // Act
            var result = item.AssignToNoone();

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().BeNull();
        }
        #endregion
    }
}
