using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace UniTodo.Tests.TodoModuleTests.Domain
{
    public class TodoItemTests
    {
        private readonly UserId _ownerId = new UserId(Guid.NewGuid());
        private readonly UserId _assignedUserId = new UserId(Guid.NewGuid());
        private readonly UserId _otherUserId = new UserId(Guid.NewGuid());

        private TodoListRun CreateRun(UserId ownerId)
        {
            return new TodoListRun("Test Run", ResetPolicy.None, false, ownerId);
        }

        private void SetRunOnItem(TodoItem item, TodoListRun run)
        {
            var property = typeof(TodoItem).GetProperty("Run", BindingFlags.Public | BindingFlags.Instance);
            property?.SetValue(item, run);
        }

        [Fact]
        public void Constructor_ShouldInitializeItemWithGivenDescription()
        {
            // Arrange
            var description = new TodoItemDescription("Test Description");

            // Act
            var item = new TodoItem(description);

            // Assert
            item.Description.Should().Be(description);
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
            item.AssignedTo.Should().BeNull();
        }

        #region MarkComplete Tests
        [Fact]
        public void MarkComplete_WhenAssignedToUser_ShouldMarkCompleteAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var result = item.MarkComplete(_assignedUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            item.CompletedBy.Should().Be(_assignedUserId);
        }

        [Fact]
        public void MarkComplete_WhenAssignedToNobodyAndActorIsOwner_ShouldMarkCompleteAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var result = item.MarkComplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            item.CompletedBy.Should().Be(_ownerId);
        }

        [Fact]
        public void MarkComplete_WhenAssignedToOtherUserAndActorIsOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var result = item.MarkComplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkComplete_WhenAssignedToNobodyAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var result = item.MarkComplete(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkComplete_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var result = item.MarkComplete(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkComplete_WhenAlreadyCompleted_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var result = item.MarkComplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This item is already marked complete.");
        }
        #endregion

        #region MarkIncomplete Tests
        [Fact]
        public void MarkIncomplete_WhenAssignedToUser_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            var result = item.MarkIncomplete(_assignedUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToNobodyAndActorIsOwner_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var result = item.MarkIncomplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToOtherUserAndActorIsOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            var result = item.MarkIncomplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToNobodyAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var result = item.MarkIncomplete(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            var result = item.MarkIncomplete(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MarkIncomplete_WhenAlreadyIncomplete_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var result = item.MarkIncomplete(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This item is still incomplete.");
        }
        #endregion

        #region UpdateNotes Tests
        [Fact]
        public void UpdateNotes_WhenAssignedToUser_ShouldUpdateNotesAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            var notes = new TodoItemNotes("New Notes");

            // Act
            var result = item.UpdateNotes(notes, _assignedUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToNobodyAndActorIsOwner_ShouldUpdateNotesAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            var notes = new TodoItemNotes("New Notes");

            // Act
            var result = item.UpdateNotes(notes, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToOtherUserAndActorIsOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var result = item.UpdateNotes(new TodoItemNotes("Notes"), _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToNobodyAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var result = item.UpdateNotes(new TodoItemNotes("Notes"), _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var result = item.UpdateNotes(new TodoItemNotes("Notes"), _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void UpdateNotes_WhenAuthorizedAndNotesEmptyString_ShouldSetNotesToNullAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            var notes = new TodoItemNotes("");

            // Act
            var result = item.UpdateNotes(notes, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().BeNull();
        }
        #endregion

        [Fact]
        public void ChangeDescription_ShouldUpdateDescription()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Old"));
            var newDescription = new TodoItemDescription("New");

            // Act
            item.ChangeDescription(newDescription);

            // Assert
            item.Description.Should().Be(newDescription);
        }

        #region SetAsignedTo Tests
        [Fact]
        public void SetAsignedTo_WhenIncomplete_ShouldSetAssignedToAndReturnSuccess()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));

            // Act
            var result = item.AssignTo(_assignedUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().Be(_assignedUserId);
        }

        [Fact]
        public void SetAsignedTo_WhenComplete_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var result = item.AssignTo(_assignedUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Couldn't asign a completed task.");
        }
        #endregion
    }
}
