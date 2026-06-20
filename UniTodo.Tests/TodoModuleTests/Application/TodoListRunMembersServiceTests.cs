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
    public class TodoListRunMembersServiceTests
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TodoListRunMembersService _service;
        private readonly UserId _currentUserId;

        public TodoListRunMembersServiceTests()
        {
            _runRepository = Substitute.For<ITodoListRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListRunMembersService(_runRepository, _userContext, _unitOfWork);
        }

        #region Helpers
        private void SetStatus(TodoListRun run, TodoListRunStatus status)
        {
            typeof(TodoListRun).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, status);
        }

        private TodoListRun CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new TodoListRun(name, ResetPolicy.Daily, isShared, ownerId ?? _currentUserId);
        }
        #endregion

        #region GetTodoListRunMembersAsync
        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenUserIsMember_ShouldReturnSuccessWithMappedMembers()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var otherUserId = new UserId(Guid.NewGuid());
            run.AddMember(otherUserId, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2)
    .And.Contain(m => m.UserId == _currentUserId.Value)
    .And.Contain(m => m.UserId == otherUserId.Value);
        }

        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenUserIsNotMember_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.GetTodoListRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region AddMemberToTodoListRunAsync
        [Fact]
        public async Task AddMemberToTodoListRunAsync_WhenAuthorizedAndValid_ShouldAddAndReturnSuccessWithDto()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var newMemberId = Guid.NewGuid();
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = newMemberId }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UserId.Should().Be(newMemberId);
            run.Members.Should().Contain(m => m.UserId.Value == newMemberId);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMemberToTodoListRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AddMemberToTodoListRunAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region RemoveMemberFromTodoListRunAsync
        [Fact]
        public async Task RemoveMemberFromTodoListRunAsync_WhenAuthorizedAndValid_ShouldRemoveAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.RemoveMemberFromTodoListRunAsync(1, memberId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Members.Should().NotContain(m => m.UserId.Value == memberId);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveMemberFromTodoListRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.RemoveMemberFromTodoListRunAsync(1, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task RemoveMemberToTodoListRunAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act 
            var result = await _service.RemoveMemberFromTodoListRunAsync(1, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion
    }
}
