using FluentAssertions;
using NSubstitute;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests.TodoModuleTests.Application
{
    public class RunPermissionsServiceTests
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RunPermissionsService _service;
        private readonly UserId _currentUserId;

        public RunPermissionsServiceTests()
        {
            _runRepository = Substitute.For<IRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new RunPermissionsService(_runRepository, _userContext, _unitOfWork);
        }

        #region Helpers
        private static void SetStatus(Run run, TodoListRunStatus status) => TestHelpers.SetStatus(run, status);

        private Run CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new Run(name, ResetPolicy.Daily, isShared, ownerId ?? _currentUserId);
        }
        #endregion

        #region GetRunPermissionsAsync
        [Fact]
        public async Task GetRunPermissionsAsync_WhenMember_ShouldReturnPermissionsDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunPermissionsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.MemberAllowedToAddItems.Should().Be(run.Permissions.MemberAllowedToAddItems);
            result.Value.MemberAllowedToRemoveItems.Should().Be(run.Permissions.MemberAllowedToRemoveItems);
        }

        [Fact]
        public async Task GetRunPermissionsAsync_WhenRunNotFound_ShouldReturnEntityNotFound()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.GetRunPermissionsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task GetRunPermissionsAsync_WhenNotMember_ShouldReturnNotAuthorized()
        {
            // Arrange
            var run = CreateActiveRun(ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunPermissionsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }
        #endregion

        #region UpdateRunPermissionsAsync
        [Fact]
        public async Task UpdateRunPermissionsAsync_WhenAuthorized_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateRunPermissionsDto
            {
                MemberAllowedToAddItems = true,
                MemberAllowedToRemoveItems = true,
                MemberAllowedToChangeDescriptions = true,
                MemberAllowedToCompleteUnassignedItems = true,
                MemberAllowedToMarkIncompleteUnassignedItems = true,
                MemberAllowedToModifyNotesForUnassignedItems = true
            };

            // Act
            var result = await _service.UpdateRunPermissionsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.MemberAllowedToAddItems.Should().BeTrue();
            result.Value.MemberAllowedToRemoveItems.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateRunPermissionsAsync_WhenRunNotFound_ShouldReturnEntityNotFound()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);
            var dto = new UpdateRunPermissionsDto
            {
                MemberAllowedToCompleteUnassignedItems = false,
                MemberAllowedToMarkIncompleteUnassignedItems = false,
                MemberAllowedToChangeDescriptions = false,
                MemberAllowedToModifyNotesForUnassignedItems = false,
                MemberAllowedToAddItems = false,
                MemberAllowedToRemoveItems = false
            };

            // Act
            var result = await _service.UpdateRunPermissionsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task UpdateRunPermissionsAsync_WhenClosedRun_ShouldReturnInvalidOperation()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateRunPermissionsDto
            {
                MemberAllowedToCompleteUnassignedItems = false,
                MemberAllowedToMarkIncompleteUnassignedItems = false,
                MemberAllowedToChangeDescriptions = false,
                MemberAllowedToModifyNotesForUnassignedItems = false,
                MemberAllowedToAddItems = true,
                MemberAllowedToRemoveItems = false
            };

            // Act
            var result = await _service.UpdateRunPermissionsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run's permissions cannot be updated.");
        }
        #endregion
    }
}
