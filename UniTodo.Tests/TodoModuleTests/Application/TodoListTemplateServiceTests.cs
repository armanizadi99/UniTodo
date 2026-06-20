using FluentAssertions;
using FluentAssertions.Specialized;
using NSubstitute;
using System.Linq.Expressions;
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
    public class TodoListTemplateServiceTests
    {
        private readonly ITodoListTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly TodoListTemplateService _service;
        private readonly UserId _currentUserId;

        public TodoListTemplateServiceTests()
        {
            _repository = Substitute.For<ITodoListTemplateRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userContext = Substitute.For<IUserContext>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListTemplateService(_repository, _unitOfWork, _userContext);
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_ShouldCreateAndReturnSuccessWithDto()
        {
            // Arrange
            var dto = new CreateTodoListTemplateDto { Name = "New List", ResetPolicy = ResetPolicy.Daily };

            // Act
            var result = await _service.CreateTodoListTemplateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be(dto.Name);
            result.Value.ResetPolicy.Should().Be(dto.ResetPolicy.Value);

            await _repository.Received(1).AddAsync(Arg.Any<TodoListTemplate>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetUserTodoListsAsync_ShouldReturnSuccessWithListsForCurrentUser()
        {
            // Arrange
            var todoLists = new List<TodoListTemplate>
            {
                new TodoListTemplate(_currentUserId, "List 1", ResetPolicy.Daily),
                new TodoListTemplate(_currentUserId, "List 2", ResetPolicy.Weekly)
            };
            _repository.GetUserTodoListTemplatesAsync(Arg.Is<Guid>(v => v == _currentUserId.Value))
                .Returns(todoLists);

            // Act
            var result = await _service.GetUserTodoListsAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().Contain(r => r.Name == "List 1");
            result.Value.Should().Contain(r => r.Name == "List 2");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnSuccessWithDto_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "Test List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.GetTodoListTemplateByIdAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Name.Should().Be("Test List");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnEntityNotFoundError_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.GetTodoListTemplateByIdAsync(99);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Be("'TodoListTemplate' with id 99' is not found.");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnNotAuthorizedError_WhenNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.GetTodoListTemplateByIdAsync(1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_ShouldReturnDuplicateEntitiesError_WhenNameAlreadyExistsCaseInsensitive()
        {
            // Arrange
            var dto = new CreateTodoListTemplateDto { Name = "New Unique List", ResetPolicy = ResetPolicy.Daily };
            _repository.IsNameDuplicateAsync(Arg.Is<string>(s => s == dto.Name))
    .Returns(true);

            // Act
            var result = await _service.CreateTodoListTemplateAsync(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.DuplicateEntities);
            result.Error.Message.Should().Be("This TodoListTemplate already exists.");
        }

        [Fact]
        public async Task DeleteTodoListAsync_ShouldDeleteAndReturnSuccess_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Delete", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.DeleteTodoListAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _repository.Received(1).Remove(todoList);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoListAsync_ShouldReturnNotAuthorizedError_WhenNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.DeleteTodoListAsync(1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
            result.Error.Message.Should().Be("");
        }

        [Fact]
        public async Task ArchiveAsync_ShouldArchiveAndSaveAndReturnSuccess_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Archive", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.ArchiveAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Archived);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ArchiveAsync_ShouldReturnEntityNotFoundError_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.ArchiveAsync(99);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Be("'TodoListTemplate' with id 99' is not found.");
        }

        [Fact]
        public async Task MakeActiveAsync_ShouldMakeActiveAndSaveAndReturnSuccess_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Activate", ResetPolicy.Daily);
            todoList.Archive(_currentUserId);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.MakeActiveAsync(1);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.Status.Should().Be(TodoListStatus.Active);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeActiveAsync_ShouldReturnEntityNotFoundError_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                            .Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.MakeActiveAsync(99);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
            result.Error.Message.Should().Be("'TodoListTemplate' with id 99' is not found.");
        }

    }
}
