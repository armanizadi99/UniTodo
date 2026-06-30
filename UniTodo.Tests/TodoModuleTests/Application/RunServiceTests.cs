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
    public class RunServiceTests
    {
        private readonly IRunRepository _runRepository;
        private readonly ITodoListTemplateRepository _templateRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RunService _service;
        private readonly UserId _currentUserId;

        public RunServiceTests()
        {
            _runRepository = Substitute.For<IRunRepository>();
            _templateRepository = Substitute.For<ITodoListTemplateRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new RunService(_runRepository, _templateRepository, _userContext, _unitOfWork);
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

        #region CreateRunFromTemplateAsync
        [Fact]
        public async Task CreateRunFromTemplateAsync_WhenUserIsOwner_ShouldCreateRunAndReturnSuccessWithMappedDto()
        {
            // Arrange
            var templateId = 1;
            var template = new TodoListTemplate(_currentUserId, "Template Name", ResetPolicy.Daily);
            template.AddTodoItemTemplate(new TodoItemTemplate(1, new TodoItemDescription("Item 1")), _currentUserId);

            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act
            var result = await _service.CreateRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(template.Name);
            result.Value.ResetPolicy.Should().Be(template.ResetPolicy);
            result.Value.OwnerId.Should().Be(_currentUserId.Value);
            result.Value.Status.Should().Be(TodoListRunStatus.Active);
            await _runRepository.Received(1).AddAsync(Arg.Any<Run>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateRunFromTemplateAsync_WhenUserIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var templateId = 1;
            var otherUser = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(otherUser, "Other Template", ResetPolicy.Daily);
            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act
            var result = await _service.CreateRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task CreateRunFromTemplateAsync_WhenTemplateNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _templateRepository.GetTodoListTemplateByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.CreateRunFromTemplateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region CreateRunAsync
        [Fact]
        public async Task CreateRunAsync_WhenValidRequest_ShouldCreatePrivateRunAndReturnSuccessWithDto()
        {
            // Arrange
            var dto = new CreateRunDto { Name = "New Run", ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.CreateRunAsync(dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(dto.Name);
            result.Value.ResetPolicy.Should().Be(dto.ResetPolicy);
            result.Value.IsShared.Should().BeFalse();
            await _runRepository.Received(1).AddAsync(Arg.Any<Run>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }
        #endregion

        #region GetUserActiveRunsAsync
        [Fact]
        public async Task GetUserActiveRunsAsync_WhenCalled_ShouldReturnSuccessWithActiveRunsForCurrentUser()
        {
            // Arrange
            var runs = new List<Run> { CreateActiveRun("Run 1") };
            _runRepository.GetUserActiveRunsAsync(_currentUserId.Value, Arg.Any<CancellationToken>()).Returns(runs);

            // Act
            var result = await _service.GetUserActiveRunsAsync(CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Name.Should().Be("Run 1");
        }
        #endregion

        #region GetRunByIdAsync
        [Fact]
        public async Task GetRunByIdAsync_WhenMember_ShouldReturnSuccessWithMappedDto()
        {
            // Arrange
            var run = CreateActiveRun("Run 1");
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunByIdAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Run 1");
        }

        [Fact]
        public async Task GetRunByIdAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.GetRunByIdAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task GetRunByIdAsync_WhenNotMember_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var run = CreateActiveRun("Run 1", ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetRunByIdAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }
        #endregion

        #region MakeRunSharedAsync
        [Fact]
        public async Task MakeRunSharedAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeRunSharedAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeRunSharedAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.MakeRunSharedAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        [Fact]
        public async Task MakeRunSharedAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeRunSharedAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region MakeRunPrivateAsync
        [Fact]
        public async Task MakeRunPrivateAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeRunPrivateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeRunPrivateAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.MakeRunPrivateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MakeRunPrivateAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeRunPrivateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region CloseRunAsync
        [Fact]
        public async Task CloseRunAsync_WhenAuthorizedAndValid_ShouldCloseAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.CloseRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Status.Should().Be(TodoListRunStatus.Closed);
            run.ClosedAt.Should().NotBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CloseRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.CloseRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task CloseRunAsync_WhenAlreadyClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.CloseRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("The run is already closed.");
        }
        #endregion

        #region ResetRunAsync
        [Fact]
        public async Task ResetRunAsync_WhenAuthorizedAndValid_ShouldResetAndReturnSuccess()
        {
            // Arrange
            var run = new Run("Test Run", ResetPolicy.None, false, _currentUserId);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.ResetRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.Iterations.Should().HaveCount(2);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResetRunAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);

            // Act
            var result = await _service.ResetRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task ResetRunAsync_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = new Run("Test Run", ResetPolicy.None, false, _currentUserId);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.ResetRunAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run cannot be reset.");
        }
        #endregion

        #region UpdateRunResetPolicyAsync
        [Fact]
        public async Task UpdateRunResetPolicyAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateResetPolicyDto { ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.UpdateRunResetPolicyAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.ResetPolicy.Should().Be(ResetPolicy.Weekly);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateRunResetPolicyAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((Run)null!);
            var dto = new UpdateResetPolicyDto { ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.UpdateRunResetPolicyAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task UpdateRunResetPolicyAsync_WhenClosed_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new UpdateResetPolicyDto { ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.UpdateRunResetPolicyAsync(1, dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run's policy cannot be updated.");
        }
        #endregion
    }
}
