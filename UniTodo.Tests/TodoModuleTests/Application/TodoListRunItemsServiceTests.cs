using FluentAssertions;
using NSubstitute;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using System.Reflection;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Application
{
    public class TodoListRunItemsServiceTests
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TodoListRunItemsService _service;
        private readonly UserId _currentUserId;

        public TodoListRunItemsServiceTests()
        {
            _runRepository = Substitute.For<ITodoListRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListRunItemsService(_runRepository, _userContext, _unitOfWork);
        }

        #region Helpers
        private void SetId<T>(T entity, int id) where T : EntityBase<int>
        {
            typeof(EntityBase<int>).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, id);
        }

        private void SetStatus(TodoListRun run, TodoListRunStatus status)
        {
            typeof(TodoListRun).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, status);
        }

        private void SetRun(TodoItem item, TodoListRun run)
        {
            typeof(TodoItem).GetField("<Run>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(item, run);
        }

        private TodoListRun CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new TodoListRun(name, ResetPolicy.Daily, isShared, ownerId ?? _currentUserId);
        }
        #endregion

        #region AddTodoItemToTodoListRunAsync
        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenAuthorizedAndActive_ShouldAddItemAndReturnSuccessWithDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new AddTodoItemDto { Description = "New Item" };

            // Act
            var result = await _service.AddTodoItemToTodoListRunAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Description.Should().Be(dto.Description);
            run.TodoItems.Should().Contain(i => i.Description.Value == dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.AddTodoItemToTodoListRunAsync(1, new AddTodoItemDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenRunIsClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddTodoItemToTodoListRunAsync(1, new AddTodoItemDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Items couldn't be added to a closed run.");
        }
        #endregion

        #region DeleteTodoItemFromTodoListRunAsync
        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenAuthorizedAndExists_ShouldRemoveItemAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.TodoItems.Should().NotContain(item);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenItemCorrelationFails_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act
            var result = await _service.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Match($"*'{nameof(TodoItem)}'*id 10'*");
        }
        #endregion

        #region GetTodoListRunItemsAsync
        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenUserIsMember_ShouldReturnSuccessWithMappedItems()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true, ownerId: new UserId(Guid.NewGuid()));
            run.AddMember(_currentUserId, run.ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("description")), run.ownerId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(1)
            .And.Contain(i => i.Description == "description");
            await _runRepository.Received(1).GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenUserIsNotMember_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.GetTodoListRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region MarkTodoItemCompleteAsync
        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenAuthorizedAndValid_ShouldMarkCompleteAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenRunIsClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region MarkTodoItemIncompleteAsync
        [Fact]
        public async Task MarkTodoItemIncompleteAsync_WhenAuthorizedAndValid_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            item.MarkComplete(_currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkTodoItemIncompleteAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MarkTodoItemIncompleteAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            SetStatus(runWithoutItem, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act 
            var result = await _service.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region UpdateNotesForTodoItemAsync
        [Fact]
        public async Task UpdateNotesForTodoItemAsync_WhenAuthorizedAndValid_ShouldUpdateNotesAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "New Notes" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes?.Value.Should().Be("New Notes");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateNotesForTodoItemAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task UpdateNotesForTodoItemAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "new notes" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region AsignMemberToItemAsync
        [Fact]
        public async Task AsignMemberToItemAsync_WhenAuthorizedAndValid_ShouldAssignAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = memberId }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().Be(new UserId(memberId));
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AsignMemberToItemAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AsignMemberToItemAsync_WhenMemberNotInRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("An item couldn't get asigned to someone that is not a member of the run.");
        }
        #endregion

        #region ChangeTodoItemDescriptionAsync
        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Old"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "New" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Description.Value.Should().Be("New");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act 
            var result = await _service.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "new desc" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion
    }
}
