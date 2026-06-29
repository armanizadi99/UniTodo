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
    public class RunMembersServiceTests
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RunMembersService _service;
        private readonly UserId _currentUserId;

        public RunMembersServiceTests()
        {
            _runRepository = Substitute.For<IRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new RunMembersService(_runRepository, _userContext, _unitOfWork);
        }

        #region Helpers
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

        #region GetRunMembersAsync
        [Fact]
        public async Task GetRunMembersAsync_WhenUserIsMember_ShouldReturnSuccessWithMappedMembers()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var otherUserId = new UserId(Guid.NewGuid());
            run.AddMember(otherUserId, _currentUserId);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2)
    .And.Contain(m => m.UserId == _currentUserId.Value)
    .And.Contain(m => m.UserId == otherUserId.Value);
        }

        [Fact]
        public async Task GetRunMembersAsync_WhenUserIsNotMember_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task GetRunMembersAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.GetRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region AddMemberToRunAsync
        [Fact]
        public async Task AddMemberToRunAsync_WhenAuthorizedAndValid_ShouldAddAndReturnSuccessWithDto()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var newMemberId = Guid.NewGuid();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddMemberToRunAsync(1, new AddMemberToRunDto { UserId = newMemberId }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.UserId.Should().Be(newMemberId);
            run.Members.Should().Contain(m => m.UserId.Value == newMemberId);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMemberToRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.AddMemberToRunAsync(1, new AddMemberToRunDto { UserId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AddMemberToRunAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddMemberToRunAsync(1, new AddMemberToRunDto { UserId = Guid.NewGuid() }, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region RemoveMemberFromRunAsync
        [Fact]
        public async Task RemoveMemberFromRunAsync_WhenAuthorizedAndValid_ShouldRemoveAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.RemoveMemberFromRunAsync(1, memberId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Members.Should().NotContain(m => m.UserId.Value == memberId);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveMemberFromRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.RemoveMemberFromRunAsync(1, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task RemoveMemberFromRunAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.RemoveMemberFromRunAsync(1, Guid.NewGuid(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion
    }
}
