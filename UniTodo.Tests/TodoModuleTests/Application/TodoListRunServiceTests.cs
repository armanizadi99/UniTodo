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

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_ShouldCreateAndReturnDto()
        {
            // Arrange
            var templateId = 1;
            var template = new TodoListTemplate(_currentUserId, "Template", ResetPolicy.Daily);
            template.AddTodoItemTemplate(new TodoItemTemplate(templateId, new TodoItemDescription("Item 1")), _currentUserId);
            
            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>())
                .Returns(template);

            // Act
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(template.Name);
            await _runRepository.Received(1).AddAsync(Arg.Any<TodoListRun>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_ShouldAddAndSave()
        {
            // Arrange
            var runId = 1;
            var run = new TodoListRun("Run", ResetPolicy.Daily, false, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(runId, true, Arg.Any<CancellationToken>())
                .Returns(run);
            var dto = new AddTodoItemDto { Description = "New Item" };

            // Act
            var result = await _service.AddTodoItemToTodoListRunAsync(runId, dto, CancellationToken.None);

            // Assert
            result.Description.Should().Be(dto.Description);
            run.TodoItems.Should().Contain(i => i.Description.Value == dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkTodoItemCompleteAsync_ShouldMarkAsComplete()
        {
            // Arrange
            var runId = 1;
            var itemId = 10;
            var run = new TodoListRun("Run", ResetPolicy.Daily, false, _currentUserId);
            var item = new TodoItem(new TodoItemDescription("Item"));
            // Use reflection to set private Id for testing
            typeof(EntityBase<int>).GetProperty("Id")!.SetValue(item, itemId);
            run.AddTodoItem(item, _currentUserId);

            _runRepository.GetTodoListRunByIdAsync(runId, true, Arg.Any<CancellationToken>())
                .Returns(run);

            // Act
            await _service.MarkTodoItemCompleteAsync(runId, itemId, CancellationToken.None);

            // Assert
            item.IsCompleted.Should().BeTrue();
            item.CompletedAt.Should().NotBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMemberToTodoListRunAsync_ShouldAddMember()
        {
            // Arrange
            var runId = 1;
            var newMemberId = Guid.NewGuid();
            var run = new TodoListRun("Run", ResetPolicy.Daily, false, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(runId, false, Arg.Any<CancellationToken>())
                .Returns(run);
            var dto = new AddMemberToTodoListRunDto { UserId = newMemberId };

            // Act
            var result = await _service.AddMemberToTodoListRunAsync(runId, dto, CancellationToken.None);

            // Assert
            result.UserId.Should().Be(newMemberId);
            run.Members.Should().Contain(m => m.Value == newMemberId);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_ShouldThrowNotAuthorized_WhenUserNotOwnerOfTemplate()
        {
            // Arrange
            var templateId = 1;
            var otherUser = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(otherUser, "Other Template", ResetPolicy.Daily);
            
            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>())
                .Returns(template);

            // Act & Assert
            await _service.Invoking(s => s.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }
    }
}
