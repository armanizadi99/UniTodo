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
    public class TodoListRunServiceTests
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly ITodoListTemplateRepository _templateRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TodoListRunService _service;
        private readonly UserId _currentUserId;

        public TodoListRunServiceTests()
        {
            _runRepository = Substitute.For<ITodoListRunRepository>();
            _templateRepository = Substitute.For<ITodoListTemplateRepository>();
            _userContext = Substitute.For<IUserContext>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListRunService(_runRepository, _templateRepository, _userContext, _unitOfWork);
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

        #region CreateTodoListRunFromTemplateAsync
        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenUserIsOwner_ShouldCreateRunAndReturnSuccessWithMappedDto()
        {
            // Arrange
            var templateId = 1;
            var template = new TodoListTemplate(_currentUserId, "Template Name", ResetPolicy.Daily);
            template.AddTodoItemTemplate(new TodoItemTemplate(1, new TodoItemDescription("Item 1")), _currentUserId);

            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(template.Name);
            result.Value.ResetPolicy.Should().Be(template.ResetPolicy);
            result.Value.OwnerId.Should().Be(_currentUserId.Value);
            result.Value.Status.Should().Be(TodoListRunStatus.Active);
            await _runRepository.Received(1).AddAsync(Arg.Any<TodoListRun>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenUserIsNotOwner_ShouldReturnNotAuthorizedError()
        {
            // Arrange
            var templateId = 1;
            var otherUser = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(otherUser, "Other Template", ResetPolicy.Daily);
            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenTemplateNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            _templateRepository.GetTodoListTemplateByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.CreateTodoListRunFromTemplateAsync(1, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region CreateTodoListRunAsync
        [Fact]
        public async Task CreateTodoListRunAsync_WhenValidRequest_ShouldCreatePrivateRunAndReturnSuccessWithDto()
        {
            // Arrange
            var dto = new CreateTodoListRunDto { Name = "New Run", ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.CreateTodoListRunAsync(dto, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(dto.Name);
            result.Value.ResetPolicy.Should().Be(dto.ResetPolicy);
            result.Value.IsShared.Should().BeFalse();
            await _runRepository.Received(1).AddAsync(Arg.Any<TodoListRun>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }
        #endregion

        #region GetUserActiveTodoRunsAsync
        [Fact]
        public async Task GetUserActiveTodoRunsAsync_WhenCalled_ShouldReturnSuccessWithActiveRunsForCurrentUser()
        {
            // Arrange
            var runs = new List<TodoListRun> { CreateActiveRun("Run 1") };
            _runRepository.GetUserActiveRunsAsync(_currentUserId.Value, Arg.Any<CancellationToken>()).Returns(runs);

            // Act
            var result = await _service.GetUserActiveTodoRunsAsync(CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value[0].Name.Should().Be("Run 1");
        }
        #endregion

        #region GetTodoListRunByRunIdAsync
        [Fact]
        public async Task GetTodoListRunByRunIdAsync_WhenRunExists_ShouldReturnSuccessWithMappedDto()
        {
            // Arrange
            var run = CreateActiveRun("Test Run");
            _runRepository.GetTodoListRunByRunIdAsync(run.RunId, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunByRunIdAsync(run.RunId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(run.Id);
            result.Value.RunId.Should().Be(run.RunId);
            result.Value.Name.Should().Be(run.Name);
            result.Value.ResetPolicy.Should().Be(run.ResetPolicy);
            result.Value.OwnerId.Should().Be(run.ownerId.Value);
            result.Value.Status.Should().Be(run.Status);
            result.Value.IsShared.Should().Be(run.IsShared);
        }

        [Fact]
        public async Task GetTodoListRunByRunIdAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var runId = Guid.NewGuid();
            _runRepository.GetTodoListRunByRunIdAsync(runId, false, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.GetTodoListRunByRunIdAsync(runId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        #endregion

        #region MakeTodoListRunSharedAsync
        [Fact]
        public async Task MakeTodoListRunSharedAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            _runRepository.GetTodoListRunByRunIdAsync(run.RunId, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeTodoListRunSharedAsync(run.RunId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeTodoListRunSharedAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var runId = Guid.NewGuid();
            _runRepository.GetTodoListRunByRunIdAsync(runId, false, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.MakeTodoListRunSharedAsync(runId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }
        [Fact]
        public async Task MakeTodoListRunSharedAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByRunIdAsync(run.RunId, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act 
            var result = await _service.MakeTodoListRunSharedAsync(run.RunId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion

        #region MakeTodoListRunPrivateAsync
        [Fact]
        public async Task MakeTodoListRunPrivateAsync_WhenAuthorizedAndValid_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            _runRepository.GetTodoListRunByRunIdAsync(run.RunId, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.MakeTodoListRunPrivateAsync(run.RunId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            run.IsShared.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeTodoListRunPrivateAsync_WhenRunNotFound_ShouldReturnEntityNotFoundError()
        {
            // Arrange
            var runId = Guid.NewGuid();
            _runRepository.GetTodoListRunByRunIdAsync(runId, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act
            var result = await _service.MakeTodoListRunPrivateAsync(runId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task MakeTodoListRunPrivateAsync_ClosedRun_ShouldReturnInvalidOperationError()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByRunIdAsync(run.RunId, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act 
            var result = await _service.MakeTodoListRunPrivateAsync(run.RunId, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.InvalidOperation);
            result.Error.Message.Should().Be("A closed run couldn't get modified.");
        }
        #endregion
    }
}
