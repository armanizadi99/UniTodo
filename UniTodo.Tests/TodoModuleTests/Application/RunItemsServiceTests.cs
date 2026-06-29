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
    public class RunItemsServiceTests
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RunItemsService _service;
        private readonly UserId _currentUserId;

        public RunItemsServiceTests()
        {
            _runRepository = Substitute.For<IRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new RunItemsService(_runRepository, _userContext, _unitOfWork);
        }

        #region Helpers
        private void SetId<T>(T entity, int id) where T : EntityBase<int>
        {
            typeof(EntityBase<int>).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, id);
        }

        private void SetStatus(Run run, TodoListRunStatus status)
        {
            typeof(Run).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, status);
        }

        private Run CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new Run(name, ResetPolicy.Daily, isShared, ownerId ?? _currentUserId);
        }
        #endregion

        #region AddRunItemToRunAsync
        [Fact]
        public async Task AddRunItemToRunAsync_WhenAuthorizedAndActive_ShouldAddItemAndReturnSuccessWithDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new AddRunItemDto { Description = "New Item" };

            // Act
            var result = await _service.AddRunItemToRunAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Description.Should().Be(dto.Description);
            run.CurrentIteration.RunItems.Should().Contain(i => i.Description.Value == dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddRunItemToRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.AddRunItemToRunAsync(1, new AddRunItemDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AddRunItemToRunAsync_WhenRunIsClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddRunItemToRunAsync(1, new AddRunItemDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("Items couldn't be added to a closed run.");
        }
        #endregion

        #region DeleteRunItemFromRunAsync
        [Fact]
        public async Task DeleteRunItemFromRunAsync_WhenAuthorizedAndExists_ShouldRemoveItemAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.DeleteRunItemFromRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.CurrentIteration.RunItems.Should().NotContain(item);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteRunItemFromRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.DeleteRunItemFromRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task DeleteRunItemFromRunAsync_WhenItemCorrelationFails_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act
            var result = await _service.DeleteRunItemFromRunAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Match($"*'{nameof(RunItem)}'*id 10'*");
        }
        #endregion

        #region GetRunItemsAsync
        [Fact]
        public async Task GetRunItemsAsync_WhenUserIsMember_ShouldReturnSuccessWithMappedItems()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true, ownerId: new UserId(Guid.NewGuid()));
            run.AddMember(_currentUserId, run.ownerId);
            run.AddRunItem(new RunItem(new TodoItemDescription("description")), run.ownerId);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().HaveCount(1)
            .And.Contain(i => i.Description == "description");
            await _runRepository.Received(1).GetRunByIdAsync(1, true, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetRunItemsAsync_WhenUserIsNotMember_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task GetRunItemsAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.GetRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region MarkRunItemCompleteAsync
        [Fact]
        public async Task MarkRunItemCompleteAsync_WhenAuthorizedAndValid_ShouldMarkCompleteAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkRunItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkRunItemCompleteAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.MarkRunItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MarkRunItemCompleteAsync_WhenRunIsClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkRunItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region MarkRunItemIncompleteAsync
        [Fact]
        public async Task MarkRunItemIncompleteAsync_WhenAuthorizedAndValid_ShouldMarkIncompleteAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            item.MarkComplete(_currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MarkRunItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.IsCompleted.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkRunItemIncompleteAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.MarkRunItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MarkRunItemIncompleteAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            SetStatus(runWithoutItem, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act
            var result = await _service.MarkRunItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region UpdateNotesForRunItemAsync
        [Fact]
        public async Task UpdateNotesForRunItemAsync_WhenAuthorizedAndValid_ShouldUpdateNotesAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.UpdateNotesForRunItemAsync(1, 10, new UpdateNotesForRunItemDto { Notes = "New Notes" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Notes?.Value.Should().Be("New Notes");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateNotesForRunItemAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.UpdateNotesForRunItemAsync(1, 10, new UpdateNotesForRunItemDto { Notes = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task UpdateNotesForRunItemAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.UpdateNotesForRunItemAsync(1, 10, new UpdateNotesForRunItemDto { Notes = "new notes" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region AssignItemToMemberAsync
        [Fact]
        public async Task AssignItemToMemberAsync_WhenAuthorizedAndValid_ShouldAssignAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignRunItemToMemberDto { MemberId = memberId }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.AssignedTo.Should().Be(new UserId(memberId));
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AssignItemToMemberAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignRunItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AssignItemToMemberAsync_WhenMemberNotInRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var item = new RunItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AssignItemToMemberAsync(1, 10, new AssignRunItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("An item couldn't get asigned to someone that is not a member of the run.");
        }
        #endregion

        #region ChangeRunItemDescriptionAsync
        [Fact]
        public async Task ChangeRunItemDescriptionAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new RunItem(new TodoItemDescription("Old"));
            SetId(item, 10);
            run.AddRunItem(item, _currentUserId);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.ChangeRunItemDescriptionAsync(1, 10, new ChangeRunItemDescriptionDto { Description = "New" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            item.Description.Value.Should().Be("New");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ChangeRunItemDescriptionAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.ChangeRunItemDescriptionAsync(1, 10, new ChangeRunItemDescriptionDto { Description = "X" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task ChangeRunItemDescriptionAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.ChangeRunItemDescriptionAsync(1, 10, new ChangeRunItemDescriptionDto { Description = "new desc" }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion
    }
}
