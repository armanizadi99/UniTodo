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
    public class TodoListTemplateItemsServiceTests
    {
        private readonly ITodoListTemplateRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly TodoListTemplateItemsService _service;
        private readonly UserId _currentUserId;

        public TodoListTemplateItemsServiceTests()
        {
            _repository = Substitute.For<ITodoListTemplateRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userContext = Substitute.For<IUserContext>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListTemplateItemsService(_repository, _unitOfWork, _userContext);
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldSucceed_WhenUserIsOwner()
        {
            // Arrange
            var todoListId = 1;
            var todoList = new TodoListTemplate(_currentUserId, "My List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);
            var dto = new AddTodoItemTemplateDto { Description = "Task 1" };

            // Act
            var result = await _service.AddTodoItemTemplateAsync(todoListId, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(1);
            result.Value.Description.Should().Be(dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldReturnNotAuthorizedError_WhenUserIsNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoListId = 1;
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);
            var dto = new AddTodoItemTemplateDto { Description = "Task 1" };

            // Act
            var result = await _service.AddTodoItemTemplateAsync(todoListId, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldReturnEntityNotFoundError_WhenTodoListDoesNotExist()
        {
            // Arrange
            var dto = new AddTodoItemTemplateDto { Description = "Task 1" };
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.AddTodoItemTemplateAsync(99, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldDeleteAndSaveAndReturnSuccess_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoListTemplateId = 1;
            var todoItemTemplateId = 1;
            var todoList = new TodoListTemplate(_currentUserId, "My List", ResetPolicy.Daily);
            var todoItemTemplate = new TodoItemTemplate(1, new TodoItemDescription("Task 1"));
            typeof(EntityBase<int>).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(todoItemTemplate, todoItemTemplateId);
            todoList.AddTodoItemTemplate(todoItemTemplate, _currentUserId);

            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);

            // Act
            var result = await _service.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            todoList.TodoItemTemplates.Should().HaveCount(0);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldReturnNotAuthorizedError_WhenNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            var todoItemTemplate = new TodoItemTemplate(1, new TodoItemDescription("Task 1"));
            typeof(EntityBase<int>).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)!
    .SetValue(todoItemTemplate, 1);
            todoList.AddTodoItemTemplate(todoItemTemplate, otherUserId);

            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);

            // Act
            var result = await _service.DeleteTodoItemTemplateAsync(1, 1);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.NotAuthorized);
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldReturnEntityNotFoundError_WhenTodoItemTemplateIdDoesNotExist()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "list1", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);

            // Act
            var result = await _service.DeleteTodoItemTemplateAsync(1, 99);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldReturnEntityNotFoundError_WhenTodoListTemplateIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns((TodoListTemplate)null!);

            // Act
            var result = await _service.DeleteTodoItemTemplateAsync(1, 99);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.EntityNotFound);
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldReturnDuplicateEntitiesError_WhenDuplicateDescriptionExists_CaseInsensitive()
        {
            // Arrange
            var todoListId = 1;
            var todoList = new TodoListTemplate(_currentUserId, "My List", ResetPolicy.Daily);
            todoList.AddTodoItemTemplate(new TodoItemTemplate(todoListId, new TodoItemDescription("ExiStIng tAsk")), _currentUserId);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                    .Returns(todoList);

            var dto = new AddTodoItemTemplateDto { Description = "existing TASK" };

            // Act
            var result = await _service.AddTodoItemTemplateAsync(todoListId, dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(DomainErrorCodes.DuplicateEntities);
            result.Error.Message.Should().Be("No duplicate descriptions allowed in a TodoList.");
        }
    }
}
