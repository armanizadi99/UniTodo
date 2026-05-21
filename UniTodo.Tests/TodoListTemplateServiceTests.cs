using FluentAssertions;
using NSubstitute;
using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Application.Services;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.Enums;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using Xunit;

namespace UniTodo.Tests
{
    public class TodoListTemplateServiceTests
    {
        private readonly IRepository<TodoListTemplate> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ITodoListTemplateService _service;
        private readonly UserId _currentUserId;

        public TodoListTemplateServiceTests()
        {
            _repository = Substitute.For<IRepository<TodoListTemplate>>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userContext = Substitute.For<IUserContext>();
            _currentUserId = new UserId(Guid.NewGuid());
            _userContext.UserId.Returns(_currentUserId);

            _service = new TodoListTemplateService(_repository, _unitOfWork, _userContext);
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_ShouldCreateAndReturnDto()
        {
            // Arrange
            var dto = new CreateTodoListTemplateDto { Name = "New List", ResetPolicy = ResetPolicy.Daily };

            // Act
            var result = await _service.CreateTodoListTemplateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.name.Should().Be(dto.Name);
            result.ResetPolicy.Should().Be(dto.ResetPolicy.Value);
            
            await _repository.Received(1).AddAsync(Arg.Any<TodoListTemplate>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetUserTodoListsAsync_ShouldReturnListsForCurrentUser()
        {
            // Arrange
            var todoLists = new List<TodoListTemplate>
            {
                new TodoListTemplate(_currentUserId, "List 1", ResetPolicy.Daily),
                new TodoListTemplate(_currentUserId, "List 2", ResetPolicy.Weekly)
            };
            _repository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<TodoListTemplate, bool>>>())
                .Returns(todoLists);

            // Act
            var result = await _service.GetUserTodoListsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.name == "List 1");
            result.Should().Contain(r => r.name == "List 2");
        }
    }
}
