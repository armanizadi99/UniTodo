using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace UniTodo.Tests.TodoModuleTests.Domain
{
    public class TodoListRunTests
    {
        private readonly UserId _ownerId = new UserId(Guid.NewGuid());
        private readonly UserId _otherUserId = new UserId(Guid.NewGuid());

        private void SetStatus(TodoListRun run, TodoListRunStatus status)
        {
            var field = typeof(TodoListRun).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(run, status);
        }

        private void SetId(EntityBase entity, int id)
        {
            var field = typeof(EntityBase<int>).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(entity, id);
        }

        private void SetRun(TodoItem item, TodoListRun run)
        {
            var field = typeof(TodoItem).GetField("<Run>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(item, run);
        }

        #region Constructor Tests
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_WhenNameIsNullOrEmptyOrWhitespace_ShouldThrowArgumentException(string? name)
        {
            // Act
            var act = () => new TodoListRun(name!, ResetPolicy.None, false, _ownerId);

            // Assert
            act.Should().Throw<ArgumentException>()
.WithParameterName("name");
        }

        [Fact]
        public void Constructor_WhenResetPolicyIsUndefined_ShouldThrowArgumentException()
        {
            // Act
            var act = () => new TodoListRun("Test", (ResetPolicy)999, false, _ownerId);

            // Assert
            act.Should().Throw<ArgumentException>()
    .WithParameterName("resetPolicy");
        }

        [Fact]
        public void Constructor_WithValidValues_ShouldInitializeCorrectly()
        {
            // Arrange
            var name = "Test Run";
            var policy = ResetPolicy.Daily;
            var isShared = true;

            // Act
            var run = new TodoListRun(name, policy, isShared, _ownerId);

            // Assert
            run.Name.Should().Be(name);
            run.ResetPolicy.Should().Be(policy);
            run.IsShared.Should().Be(isShared);
            run.ownerId.Should().Be(_ownerId);
            run.Status.Should().Be(TodoListRunStatus.Active);
            run.RunId.Should().NotBeEmpty();
            run.Members.Should().Contain(m => m.UserId == _ownerId);
        }
        #endregion

        [Fact]
        public void CreateRunFromTodoItemTemplates_WithValidParameters_ShouldCreateRunAndItemsAndReturnSuccess()
        {
            // Arrange
            var templates = new List<TodoItemTemplate>
            {
                new TodoItemTemplate(1, new TodoItemDescription("Item 1")),
                new TodoItemTemplate(1, new TodoItemDescription("Item 2"))
            };

            // Act
            var result = TodoListRun.CreateRunFromTodoItemTemplates(templates, "Template Run", ResetPolicy.None, false, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TodoItems.Should().HaveCount(2);
            result.Value.TodoItems.Should().Contain(i => i.Description.Value == "Item 1");
            result.Value.TodoItems.Should().Contain(i => i.Description.Value == "Item 2");
        }

        #region AddTodoItem Tests
        [Fact]
        public void AddTodoItem_WhenAuthorizedAndActive_ShouldAddItemAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));

            // Act
            var result = run.AddTodoItem(item, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.TodoItems.Should().Contain(item);
        }

        [Fact]
        public void AddTodoItem_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.AddTodoItem(item, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Items couldn't be added to a closed run.");
        }

        [Fact]
        public void AddTodoItem_WhenNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));

            // Act
            var result = run.AddTodoItem(item, _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void AddTodoItem_WhenDuplicateDescription_ShouldReturnDuplicateEntitiesError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("Test Item")), _ownerId);
            var duplicateItem = new TodoItem(new TodoItemDescription("test item"));

            // Act
            var result = run.AddTodoItem(duplicateItem, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.DuplicateEntities);
            result.Error.Message.Should().Be("No duplicate description could be in a todo list run.");
        }
        #endregion

        #region DeleteItem Tests
        [Fact]
        public void DeleteItem_WhenExistingAndAuthorized_ShouldRemoveItemAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var result = run.DeleteItem(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.TodoItems.Should().NotContain(item);
        }

        [Fact]
        public void DeleteItem_WhenNonExisting_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var result = run.DeleteItem(999, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Be("'TodoItem' with id 999' is not found.");
        }

        [Fact]
        public void DeleteItem_WhenNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var result = run.DeleteItem(1, _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void DeleteItem_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.DeleteItem(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Items couldn't be deleted from a closed run.");
        }
        #endregion

        #region MakeShared Tests
        [Fact]
        public void MakeShared_WhenOwner_ShouldMakeSharedAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var result = run.MakeShared(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeTrue();
        }

        [Fact]
        public void MakeShared_WhenNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var result = run.MakeShared(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public void MakeShared_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.MakeShared(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }

        [Fact]
        public void MakeShared_WhenAlreadyShared_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);

            // Act
            var result = run.MakeShared(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This run is already shared.");
        }
        #endregion

        #region MakePrivate Tests
        [Fact]
        public void MakePrivate_WhenSharedAndOwner_ShouldMakePrivateAndCleanupAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            run.AssignItemToMember(1, memberId, _ownerId);

            // Act
            var result = run.MakePrivate(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeFalse();
            run.Members.Should().HaveCount(1).And.Contain(m => m.UserId == _ownerId);
            item.AssignedTo.Should().BeNull();
        }

        [Fact]
        public void MakePrivate_WhenAlreadyPrivate_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var result = run.MakePrivate(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("This run is already private.");
        }

        [Fact]
        public void MakePrivate_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.MakePrivate(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }

        [Fact]
        public void MakePrivate_WhenNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);

            // Act
            var result = run.MakePrivate(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }
        #endregion

        #region MarkItemComplete/Incomplete Tests
        [Fact]
        public void MarkItemComplete_WhenAuthorized_ShouldMarkCompleteAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);

            // Act
            var result = run.MarkItemComplete(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MarkItemComplete_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.MarkItemComplete(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }

        [Fact]
        public void MarkItemIncomplete_WhenAuthorized_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);
            run.MarkItemComplete(1, _ownerId);

            // Act
            var result = run.MarkItemIncomplete(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
        }
        #endregion

        #region UpdateNotes Tests
        [Fact]
        public void UpdateNotes_WhenAuthorized_ShouldUpdateNotesAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);
            var notes = new TodoItemNotes("New Notes");

            // Act
            var result = run.UpdateNotes(1, notes, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.UpdateNotes(1, new TodoItemNotes("Notes"), _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }

        [Fact]
        public void UpdateNotes_WhenNonExistingItem_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var result = run.UpdateNotes(999, new TodoItemNotes("Notes"), _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Be("'TodoItem' with id 999' is not found.");
        }
        #endregion

        #region AsignItemToMember Tests
        [Fact]
        public void AsignItemToMember_WhenAuthorized_ShouldAssignAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var result = run.AssignItemToMember(1, memberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().Be(memberId);
        }

        [Fact]
        public void AsignItemToMember_WhenUserNotMember_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var notMemberId = new UserId(Guid.NewGuid());
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var result = run.AssignItemToMember(1, notMemberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("An item couldn't get asigned to someone that is not a member of the run.");
        }
        #endregion

        #region ChangeItemDescription Tests
        [Fact]
        public void ChangeItemDescription_WhenAuthorized_ShouldUpdateDescriptionAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Old Description"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            var newDesc = new TodoItemDescription("New Description");

            // Act
            var result = run.ChangeItemDescription(1, newDesc, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Description.Should().Be(newDesc);
        }

        [Fact]
        public void ChangeItemDescription_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Old Description"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var result = run.ChangeItemDescription(1, new TodoItemDescription("New"), _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region AddMember/RemoveMember Tests
        [Fact]
        public void AddMember_WhenAuthorizedAndShared_ShouldAddMemberAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());

            // Act
            var result = run.AddMember(memberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Members.Should().Contain(m => m.UserId == memberId);
        }

        [Fact]
        public void AddMember_WhenAlreadyMember_ShouldReturnDuplicateEntitiesError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);

            // Act
            var result = run.AddMember(memberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.DuplicateEntities);
            result.Error.Message.Should().Be("this user is already a member of this run");
        }

        [Fact]
        public void AddMember_WhenNotShared_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var memberId = new UserId(Guid.NewGuid());

            // Act
            var result = run.AddMember(memberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Couldn't add members to a private group.");
        }

        [Fact]
        public void RemoveMember_WhenAuthorized_ShouldRemoveMemberAndUnassignTasksAndReturnSuccess()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            run.AssignItemToMember(1, memberId, _ownerId);

            // Act
            var result = run.RemoveMember(memberId, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Members.Should().NotContain(m => m.UserId == memberId);
            item.AssignedTo.Should().BeNull();
        }
        #endregion
    }
}
