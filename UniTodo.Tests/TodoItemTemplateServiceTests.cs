using FluentAssertions;
using NSubstitute;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests
{
    public class TodoItemTemplateServiceTests
    {
        private readonly IRepository<TodoItemTemplate> _todoItemTemplateRepository;
        private readonly IRepository<TodoListTemplate> _todoListRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ITodoItemTemplateService _service;
        private readonly UserId _currentUserId;

        public TodoItemTemplateServiceTests()
        {
            _todoItemTemplateRepository = Substitute.For<IRepository<TodoItemTemplate>>();
            _todoListRepository = Substitute.For<IRepository<TodoListTemplate>>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userContext = Substitute.For<IUserContext>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoItemTemplateService(_todoItemTemplateRepository, _todoListRepository, _userContext, _unitOfWork);
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldSucceed_WhenUserIsOwner()
        {
            // Arrange
            var todoListId = 1;
            var todoList = new TodoListTemplate(_currentUserId, "My List", ResetPolicy.Daily);
            _todoListRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<TodoListTemplate, bool>>>(), Arg.Any<System.Linq.Expressions.Expression<Func<TodoListTemplate, object>>[]>())
                .Returns(todoList);
            var dto = new AddTodoItemTemplateDto { TodoListId = todoListId, Description = "Task 1" };

            // Act
            await _service.AddTodoItemTemplateAsync(dto);

            // Assert
            await _todoItemTemplateRepository.Received(1).AddAsync(Arg.Any<TodoItemTemplate>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldThrow_WhenUserIsNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoListId = 1;
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _todoListRepository.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<TodoListTemplate, bool>>>(), Arg.Any<System.Linq.Expressions.Expression<Func<TodoListTemplate, object>>[]>())
                .Returns(todoList);
            var dto = new AddTodoItemTemplateDto { TodoListId = todoListId, Description = "Task 1" };

            // Act
            var act = () => _service.AddTodoItemTemplateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<DomainNotAuthorizedException>();
            await _todoItemTemplateRepository.DidNotReceive().AddAsync(Arg.Any<TodoItemTemplate>());
        }
    }
}
