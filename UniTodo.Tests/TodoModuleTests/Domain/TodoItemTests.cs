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
        public void MarkComplete_WhenAssignedToUser_ShouldMarkComplete()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            item.MarkComplete(_assignedUserId);

            // Assert
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            item.CompletedBy.Should().Be(_assignedUserId);
        }

        [Fact]
        public void MarkComplete_WhenAssignedToNobodyAndActorIsOwner_ShouldMarkComplete()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            item.MarkComplete(_ownerId);

            // Assert
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            item.CompletedBy.Should().Be(_ownerId);
        }

        [Fact]
        public void MarkComplete_WhenAssignedToOtherUserAndActorIsOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var act = () => item.MarkComplete(_ownerId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkComplete_WhenAssignedToNobodyAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var act = () => item.MarkComplete(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkComplete_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var act = () => item.MarkComplete(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkComplete_WhenAlreadyCompleted_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var act = () => item.MarkComplete(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region MarkIncomplete Tests
        [Fact]
        public void MarkIncomplete_WhenAssignedToUser_ShouldMarkIncomplete()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            item.MarkIncomplete(_assignedUserId);

            // Assert
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToNobodyAndActorIsOwner_ShouldMarkIncomplete()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            item.MarkIncomplete(_ownerId);

            // Assert
            item.IsCompleted.Should().BeFalse();
            item.CompletedAt.Should().BeNull();
            item.CompletedBy.Should().BeNull();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToOtherUserAndActorIsOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            var act = () => item.MarkIncomplete(_ownerId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToNobodyAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var act = () => item.MarkIncomplete(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkIncomplete_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            item.MarkComplete(_assignedUserId);

            // Act
            var act = () => item.MarkIncomplete(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MarkIncomplete_WhenAlreadyIncomplete_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var act = () => item.MarkIncomplete(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region UpdateNotes Tests
        [Fact]
        public void UpdateNotes_WhenAssignedToUser_ShouldUpdateNotes()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);
            var notes = new TodoItemNotes("New Notes");

            // Act
            item.UpdateNotes(notes, _assignedUserId);

            // Assert
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToNobodyAndActorIsOwner_ShouldUpdateNotes()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            var notes = new TodoItemNotes("New Notes");

            // Act
            item.UpdateNotes(notes, _ownerId);

            // Assert
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToOtherUserAndActorIsOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var act = () => item.UpdateNotes(new TodoItemNotes("Notes"), _ownerId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToNobodyAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);

            // Act
            var act = () => item.UpdateNotes(new TodoItemNotes("Notes"), _otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void UpdateNotes_WhenAssignedToOtherUserAndActorIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.AssignTo(_assignedUserId);

            // Act
            var act = () => item.UpdateNotes(new TodoItemNotes("Notes"), _otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void UpdateNotes_WhenAuthorizedAndNotesEmptyString_ShouldSetNotesToNull()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            var notes = new TodoItemNotes("");

            // Act
            item.UpdateNotes(notes, _ownerId);

            // Assert
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
        public void SetAsignedTo_WhenIncomplete_ShouldSetAssignedTo()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));

            // Act
            item.AssignTo(_assignedUserId);

            // Assert
            item.AssignedTo.Should().Be(_assignedUserId);
        }

        [Fact]
        public void SetAsignedTo_WhenComplete_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var item = new TodoItem(new TodoItemDescription("Test"));
            var run = CreateRun(_ownerId);
            SetRunOnItem(item, run);
            item.MarkComplete(_ownerId);

            // Act
            var act = () => item.AssignTo(_assignedUserId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion
    }
}
