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
            var act = () => new TodoListRun(name, ResetPolicy.None, false, _ownerId);

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
            run.Members.Should().Contain(_ownerId);
        }
        #endregion

        [Fact]
        public void CreateRunFromTodoItemTemplates_WithValidParameters_ShouldCreateRunAndItems()
        {
            // Arrange
            var templates = new List<TodoItemTemplate>
            {
                new TodoItemTemplate(1, new TodoItemDescription("Item 1")),
                new TodoItemTemplate(1, new TodoItemDescription("Item 2"))
            };

            // Act
            var run = TodoListRun.CreateRunFromTodoItemTemplates(templates, "Template Run", ResetPolicy.None, false, _ownerId);

            // Assert
            run.TodoItems.Should().HaveCount(2);
            run.TodoItems.Should().Contain(i => i.Description.Value == "Item 1");
            run.TodoItems.Should().Contain(i => i.Description.Value == "Item 2");
        }

        #region AddTodoItem Tests
        [Fact]
        public void AddTodoItem_WhenAuthorizedAndActive_ShouldAddItem()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));

            // Act
            run.AddTodoItem(item, _ownerId);

            // Assert
            run.TodoItems.Should().Contain(item);
        }

        [Fact]
        public void AddTodoItem_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.AddTodoItem(item, _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void AddTodoItem_WhenNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));

            // Act
            var act = () => run.AddTodoItem(item, _otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void AddTodoItem_WhenDuplicateDescription_ShouldThrowDomainDuplicateEntitiesException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("Test Item")), _ownerId);
            var duplicateItem = new TodoItem(new TodoItemDescription("test item"));

            // Act
            var act = () => run.AddTodoItem(duplicateItem, _ownerId);

            // Assert
            act.Should().Throw<DomainDuplicateEntitiesException>();
        }
        #endregion

        #region DeleteItem Tests
        [Fact]
        public void DeleteItem_WhenExistingAndAuthorized_ShouldRemoveItem()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            run.DeleteItem(1, _ownerId);

            // Assert
            run.TodoItems.Should().NotContain(item);
        }

        [Fact]
        public void DeleteItem_WhenNonExisting_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var act = () => run.DeleteItem(999, _ownerId);

            // Assert
            act.Should().Throw<DomainEntityNotFoundException>();
        }

        [Fact]
        public void DeleteItem_WhenNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var act = () => run.DeleteItem(1, _otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void DeleteItem_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.DeleteItem(1, _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region MakeShared Tests
        [Fact]
        public void MakeShared_WhenOwner_ShouldMakeShared()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            run.MakeShared(_ownerId);

            // Assert
            run.IsShared.Should().BeTrue();
        }

        [Fact]
        public void MakeShared_WhenNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var act = () => run.MakeShared(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }

        [Fact]
        public void MakeShared_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.MakeShared(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void MakeShared_WhenAlreadyShared_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);

            // Act
            var act = () => run.MakeShared(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region MakePrivate Tests
        [Fact]
        public void MakePrivate_WhenSharedAndOwner_ShouldMakePrivateAndCleanup()
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
            run.MakePrivate(_ownerId);

            // Assert
            run.IsShared.Should().BeFalse();
            run.Members.Should().HaveCount(1).And.Contain(_ownerId);
            item.AssignedTo.Should().BeNull();
        }

        [Fact]
        public void MakePrivate_WhenAlreadyPrivate_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var act = () => run.MakePrivate(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void MakePrivate_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.MakePrivate(_ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void MakePrivate_WhenNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);

            // Act
            var act = () => run.MakePrivate(_otherUserId);

            // Assert
            act.Should().Throw<DomainNotAuthorizedException>();
        }
        #endregion

        #region MarkItemComplete/Incomplete Tests
        [Fact]
        public void MarkItemComplete_WhenAuthorized_ShouldMarkComplete()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);

            // Act
            run.MarkItemComplete(1, _ownerId);

            // Assert
            item.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MarkItemComplete_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.MarkItemComplete(1, _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void MarkItemIncomplete_WhenAuthorized_ShouldMarkIncomplete()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);
            run.MarkItemComplete(1, _ownerId);

            // Act
            run.MarkItemIncomplete(1, _ownerId);

            // Assert
            item.IsCompleted.Should().BeFalse();
        }
        #endregion

        #region UpdateNotes Tests
        [Fact]
        public void UpdateNotes_WhenAuthorized_ShouldUpdateNotes()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetRun(item, run);
            SetId(item, 1);
            var notes = new TodoItemNotes("New Notes");

            // Act
            run.UpdateNotes(1, notes, _ownerId);

            // Assert
            item.Notes.Should().Be(notes);
        }

        [Fact]
        public void UpdateNotes_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.UpdateNotes(1, new TodoItemNotes("Notes"), _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void UpdateNotes_WhenNonExistingItem_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);

            // Act
            var act = () => run.UpdateNotes(999, new TodoItemNotes("Notes"), _ownerId);

            // Assert
            act.Should().Throw<DomainEntityNotFoundException>();
        }
        #endregion

        #region AsignItemToMember Tests
        [Fact]
        public void AsignItemToMember_WhenAuthorized_ShouldAssign()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            run.AssignItemToMember(1, memberId, _ownerId);

            // Assert
            item.AssignedTo.Should().Be(memberId);
        }

        [Fact]
        public void AsignItemToMember_WhenUserNotMember_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var notMemberId = new UserId(Guid.NewGuid());
            var item = new TodoItem(new TodoItemDescription("Test Item"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);

            // Act
            var act = () => run.AssignItemToMember(1, notMemberId, _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region ChangeItemDescription Tests
        [Fact]
        public void ChangeItemDescription_WhenAuthorized_ShouldUpdateDescription()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Old Description"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            var newDesc = new TodoItemDescription("New Description");

            // Act
            run.ChangeItemDescription(1, newDesc, _ownerId);

            // Assert
            item.Description.Should().Be(newDesc);
        }

        [Fact]
        public void ChangeItemDescription_WhenClosed_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var item = new TodoItem(new TodoItemDescription("Old Description"));
            run.AddTodoItem(item, _ownerId);
            SetId(item, 1);
            SetStatus(run, TodoListRunStatus.Closed);

            // Act
            var act = () => run.ChangeItemDescription(1, new TodoItemDescription("New"), _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }
        #endregion

        #region AddMember/RemoveMember Tests
        [Fact]
        public void AddMember_WhenAuthorizedAndShared_ShouldAddMember()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());

            // Act
            run.AddMember(memberId, _ownerId);

            // Assert
            run.Members.Should().Contain(memberId);
        }

        [Fact]
        public void AddMember_WhenAlreadyMember_ShouldThrowDomainDuplicateEntitiesException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, true, _ownerId);
            var memberId = new UserId(Guid.NewGuid());
            run.AddMember(memberId, _ownerId);

            // Act
            var act = () => run.AddMember(memberId, _ownerId);

            // Assert
            act.Should().Throw<DomainDuplicateEntitiesException>();
        }

        [Fact]
        public void AddMember_WhenNotShared_ShouldThrowDomainInvalidOperationException()
        {
            // Arrange
            var run = new TodoListRun("Test", ResetPolicy.None, false, _ownerId);
            var memberId = new UserId(Guid.NewGuid());

            // Act
            var act = () => run.AddMember(memberId, _ownerId);

            // Assert
            act.Should().Throw<DomainInvalidOperationException>();
        }

        [Fact]
        public void RemoveMember_WhenAuthorized_ShouldRemoveMemberAndUnassignTasks()
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
            run.RemoveMember(memberId, _ownerId);

            // Assert
            run.Members.Should().NotContain(memberId);
            item.AssignedTo.Should().BeNull();
        }
        #endregion
    }
}
