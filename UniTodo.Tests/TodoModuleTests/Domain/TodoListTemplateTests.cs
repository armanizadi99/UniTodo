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
        private readonly UserId _ownerId = new UserId(Guid.NewGuid());
        private readonly UserId _otherUserId = new UserId(Guid.NewGuid());
        private const string ValidName = "Test List";
        private const ResetPolicy DefaultPolicy = ResetPolicy.Daily;

        private void SetId(EntityBase entity, int id)
        {
            var field = typeof(EntityBase<int>).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(entity, id);
        }

        private TodoListTemplate CreateTemplate() => new TodoListTemplate(_ownerId, ValidName, DefaultPolicy);

        #region Constructor Tests
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Act
            var todoList = new TodoListTemplate(_ownerId, ValidName, DefaultPolicy);

            // Assert
            todoList.OwnerId.Should().Be(_ownerId);
            todoList.Name.Should().Be(ValidName);
            todoList.ResetPolicy.Should().Be(DefaultPolicy);
            todoList.Status.Should().Be(TodoListStatus.Active);
            todoList.TodoItemTemplates.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WhenResetPolicyIsInvalid_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var invalidResetPolicy = (ResetPolicy)999;

            // Act
            Action act = () => new TodoListTemplate(_ownerId, ValidName, invalidResetPolicy);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*resetPolicy*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WhenNameIsInvalid_ShouldThrowArgumentException(string? name)
        {
            // Act
            Action act = () => new TodoListTemplate(_ownerId, name!, DefaultPolicy);

            // Assert
            if (name == null)
                act.Should().Throw<ArgumentNullException>();
            else
                act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(ResetPolicy.None)]
        [InlineData(ResetPolicy.Daily)]
        [InlineData(ResetPolicy.Weekly)]
        [InlineData(ResetPolicy.Monthly)]
        public void Constructor_WithValidResetPolicies_ShouldSetCorrectPolicy(ResetPolicy policy)
        {
            // Act
            var todoList = new TodoListTemplate(_ownerId, ValidName, policy);

            // Assert
            todoList.ResetPolicy.Should().Be(policy);
        }
        #endregion

        #region Archive Tests
        [Fact]
        public void Archive_WhenActorIsOwner_ShouldSetStatusToArchived()
        {
            // Arrange
            var todoList = CreateTemplate();

            // Act
            var result = todoList.Archive(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Archived);
        }

        [Fact]
        public void Archive_WhenActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var todoList = CreateTemplate();

            // Act
            var result = todoList.Archive(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            todoList.Status.Should().Be(TodoListStatus.Active);
        }

        [Fact]
        public void Archive_WhenAlreadyArchived_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var todoList = CreateTemplate();
            todoList.Archive(_ownerId);

            // Act
            var result = todoList.Archive(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            todoList.Status.Should().Be(TodoListStatus.Archived);
        }
        #endregion

        #region MakeActive Tests
        [Fact]
        public void MakeActive_WhenActorIsOwnerAndArchived_ShouldSetStatusToActive()
        {
            // Arrange
            var todoList = CreateTemplate();
            todoList.Archive(_ownerId);

            // Act
            var result = todoList.MakeActive(_ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Active);
        }

        [Fact]
        public void MakeActive_WhenActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var todoList = CreateTemplate();
            todoList.Archive(_ownerId);

            // Act
            var result = todoList.MakeActive(_otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            todoList.Status.Should().Be(TodoListStatus.Archived);
        }

        [Fact]
        public void MakeActive_WhenAlreadyActive_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var todoList = CreateTemplate();

            // Act
            var result = todoList.MakeActive(_ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            todoList.Status.Should().Be(TodoListStatus.Active);
        }
        #endregion

        #region AddTodoItemTemplate Tests
        [Fact]
        public void AddTodoItemTemplate_WhenActorIsOwner_ShouldAddItem()
        {
            // Arrange
            var todoList = CreateTemplate();
            var itemTemplate = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));

            // Act
            var result = todoList.AddTodoItemTemplate(itemTemplate, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(1);
            todoList.TodoItemTemplates.First().Description.Value.Should().Be("Buy milk");
        }

        [Fact]
        public void AddTodoItemTemplate_WhenActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var todoList = CreateTemplate();
            var itemTemplate = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));

            // Act
            var result = todoList.AddTodoItemTemplate(itemTemplate, _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            todoList.TodoItemTemplates.Should().BeEmpty();
        }

        [Fact]
        public void AddTodoItemTemplate_WhenDescriptionAlreadyExists_ShouldReturnDuplicateEntitiesError()
        {
            // Arrange
            var todoList = CreateTemplate();
            var itemTemplate1 = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));
            var itemTemplate2 = new TodoItemTemplate(0, new TodoItemDescription("buy milk"));
            todoList.AddTodoItemTemplate(itemTemplate1, _ownerId);

            // Act
            var result = todoList.AddTodoItemTemplate(itemTemplate2, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.DuplicateEntities);
            todoList.TodoItemTemplates.Should().HaveCount(1);
        }

        [Fact]
        public void AddTodoItemTemplate_WithMultipleDifferentDescriptions_ShouldAddAll()
        {
            // Arrange
            var todoList = CreateTemplate();
            var item1 = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));
            var item2 = new TodoItemTemplate(0, new TodoItemDescription("Walk dog"));

            // Act
            todoList.AddTodoItemTemplate(item1, _ownerId);
            var result = todoList.AddTodoItemTemplate(item2, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(2);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public void Delete_WhenActorIsOwnerAndItemExists_ShouldRemoveItem()
        {
            // Arrange
            var todoList = CreateTemplate();
            var itemTemplate = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));
            SetId(itemTemplate, 42);
            todoList.AddTodoItemTemplate(itemTemplate, _ownerId);

            // Act
            var result = todoList.Delete(42, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().BeEmpty();
        }

        [Fact]
        public void Delete_WhenActorIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var todoList = CreateTemplate();
            var itemTemplate = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));
            SetId(itemTemplate, 42);
            todoList.AddTodoItemTemplate(itemTemplate, _ownerId);

            // Act
            var result = todoList.Delete(42, _otherUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            todoList.TodoItemTemplates.Should().HaveCount(1);
        }

        [Fact]
        public void Delete_WhenItemDoesNotExist_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var todoList = CreateTemplate();

            // Act
            var result = todoList.Delete(999, _ownerId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public void Delete_WithMultipleItems_ShouldOnlyDeleteMatchingItem()
        {
            // Arrange
            var todoList = CreateTemplate();
            var item1 = new TodoItemTemplate(0, new TodoItemDescription("Buy milk"));
            var item2 = new TodoItemTemplate(0, new TodoItemDescription("Walk dog"));
            SetId(item1, 1);
            SetId(item2, 2);
            todoList.AddTodoItemTemplate(item1, _ownerId);
            todoList.AddTodoItemTemplate(item2, _ownerId);

            // Act
            var result = todoList.Delete(1, _ownerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(1);
            todoList.TodoItemTemplates.First().Id.Should().Be(2);
        }
        #endregion

        [Fact]
        public void TodoItemTemplates_ShouldReturnReadOnlyCollection()
        {
            // Arrange
            var todoList = CreateTemplate();
            todoList.AddTodoItemTemplate(new TodoItemTemplate(0, new TodoItemDescription("Buy milk")), _ownerId);

            // Act
            var templates = todoList.TodoItemTemplates;

            // Assert
            templates.Should().BeAssignableTo<IReadOnlyCollection<TodoItemTemplate>>();
            templates.Should().HaveCount(1);
        }

        [Fact]
        public void MultipleOperations_ShouldMaintainStateCorrectly()
        {
            // Arrange
            var todoList = new TodoListTemplate(_ownerId, ValidName, ResetPolicy.Weekly);
            var item1 = new TodoItemTemplate(0, new TodoItemDescription("Task 1"));
            var item2 = new TodoItemTemplate(0, new TodoItemDescription("Task 2"));
            var item3 = new TodoItemTemplate(0, new TodoItemDescription("Task 3"));
            SetId(item1, 1);
            SetId(item2, 2);
            SetId(item3, 3);

            todoList.AddTodoItemTemplate(item1, _ownerId);
            todoList.AddTodoItemTemplate(item2, _ownerId);
            todoList.AddTodoItemTemplate(item3, _ownerId);

            // Act & Assert
            todoList.TodoItemTemplates.Should().HaveCount(3);

            todoList.Delete(2, _ownerId).IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(2);

            todoList.Archive(_ownerId).IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Archived);

            var item4 = new TodoItemTemplate(0, new TodoItemDescription("Task 4"));
            SetId(item4, 4);
            todoList.AddTodoItemTemplate(item4, _ownerId).IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(3);

            todoList.MakeActive(_ownerId).IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Active);
        }
    }
}
