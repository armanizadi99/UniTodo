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
    public class RunSettingsServiceTests
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RunSettingsService _service;
        private readonly UserId _currentUserId;

        public RunSettingsServiceTests()
        {
            _runRepository = Substitute.For<IRunRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new RunSettingsService(_runRepository, _userContext, _unitOfWork);
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

        #region GetRunSettingsAsync
        [Fact]
        public async Task GetRunSettingsAsync_WhenMember_ShouldReturnSettingsDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunSettingsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TimeZone.Should().Be(run.Settings.TimeZone.Id);
            result.Value.EndOfWeekDay.Should().Be(run.Settings.EndOfWeekDay);
            result.Value.PreserveHistory.Should().Be(run.Settings.PreserveHistory);
        }

        [Fact]
        public async Task GetRunSettingsAsync_WhenRunNotFound_ShouldReturnEntityNotFound()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.GetRunSettingsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task GetRunSettingsAsync_WhenNotMember_ShouldReturnNotAuthorized()
        {
            // Arrange
            var run = CreateActiveRun(ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunSettingsAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }
        #endregion

        #region UpdateRunSettingsAsync
        [Fact]
        public async Task UpdateRunSettingsAsync_WhenAuthorized_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateRunSettingsDto
            {
                TimeZone = "Tokyo Standard Time",
                EndOfWeekDay = DayOfWeek.Wednesday,
                PreserveHistory = false
            };

            // Act
            var result = await _service.UpdateRunSettingsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.TimeZone.Should().Be("Tokyo Standard Time");
            result.Value.EndOfWeekDay.Should().Be(DayOfWeek.Wednesday);
            result.Value.PreserveHistory.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateRunSettingsAsync_WhenRunNotFound_ShouldReturnEntityNotFound()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);
            var dto = new UpdateRunSettingsDto
            {
                TimeZone = "UTC",
                EndOfWeekDay = DayOfWeek.Friday,
                PreserveHistory = true
            };

            // Act
            var result = await _service.UpdateRunSettingsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task UpdateRunSettingsAsync_WhenClosedRun_ShouldReturnInvalidOperation()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateRunSettingsDto
            {
                TimeZone = "UTC",
                EndOfWeekDay = DayOfWeek.Friday,
                PreserveHistory = true
            };

            // Act
            var result = await _service.UpdateRunSettingsAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run's settings cannot be updated.");
        }
        #endregion
    }
}
