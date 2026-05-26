using FluentAssertions;
using FluentAssertions.Specialized;
using NSubstitute;
using System.Linq.Expressions;
using UniTodo.Modules.Todos.Application.Common;
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
        private readonly ITodoListTemplateService _service;
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
            _repository.GetUserTodoListTemplatesAsync(Arg.Is<Guid>(v => v == _currentUserId.Value))
                .Returns(todoLists);

            // Act
            var result = await _service.GetUserTodoListsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.name == "List 1");
            result.Should().Contain(r => r.name == "List 2");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldReturnDto_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "Test List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            var result = await _service.GetTodoListTemplateByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.name.Should().Be("Test List");
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldThrowNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListTemplateByIdAsync(99))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task GetTodoListTemplateByIdAsync_ShouldThrowNotAuthorized_WhenNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListTemplateByIdAsync(1))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task CreateTodoListTemplateAsync_ShouldThrowDuplicateName_WhenNameAlreadyExistsCaseInsensitive()
        {
            // Arrange
            var dto = new CreateTodoListTemplateDto { Name = "New Unique List", ResetPolicy = ResetPolicy.Daily };
            var existingList = new TodoListTemplate(_currentUserId, "Existing List", ResetPolicy.Daily);
            _repository.IsNameDuplicateAsync(Arg.Is<string>(s => s == dto.Name))
    .Returns(true);

            // Act & Assert
            await _service.Invoking(s => s.CreateTodoListTemplateAsync(dto))
                .Should().ThrowAsync<DomainDuplicateEntitiesException>();
        }

        [Fact]
        public async Task DeleteTodoListAsync_ShouldDelete_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Delete", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            await _service.DeleteTodoListAsync(1);

            // Assert
            _repository.Received(1).Remove(todoList);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoListAsync_ShouldThrowNotAuthorized_WhenNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoListAsync(1))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task ArchiveAsync_ShouldArchiveAndSave_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Archive", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            await _service.ArchiveAsync(1);

            // Assert
            todoList.Status.Should().Be(TodoListStatus.Archived);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ArchiveAsync_ShouldThrowNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.ArchiveAsync(99))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task MakeActiveAsync_ShouldMakeActiveAndSave_WhenFoundAndAuthorized()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "To Activate", ResetPolicy.Daily);
            todoList.Archive(_currentUserId);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act
            await _service.MakeActiveAsync(1);

            // Assert
            todoList.Status.Should().Be(TodoListStatus.Active);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeActiveAsync_ShouldThrowNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                            .Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.MakeActiveAsync(99))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
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
            todoList.TodoItemTemplates.Should().HaveCount(1);
            result.Description.Should().Be(dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldThrow_WhenUserIsNotOwner()
        {
            // Arrange
            var otherUserId = new UserId(Guid.NewGuid());
            var todoListId = 1;
            var todoList = new TodoListTemplate(otherUserId, "Other List", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                .Returns(todoList);
            var dto = new AddTodoItemTemplateDto { Description = "Task 1" };

            // Act
            var act = () => _service.AddTodoItemTemplateAsync(todoListId, dto);

            // Assert
            await act.Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldThrowNotFound_WhenTodoListDoesNotExist()
        {
            // Arrange
            var dto = new AddTodoItemTemplateDto { Description = "Task 1" };
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 99))
                .Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.AddTodoItemTemplateAsync(99, dto))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldDeleteAndSave_WhenFoundAndAuthorized()
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
            await _service.DeleteTodoItemTemplateAsync(todoListTemplateId, todoItemTemplateId);

            // Assert
            todoList.TodoItemTemplates.Should().HaveCount(0);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldThrowNotAuthorized_WhenNotOwner()
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

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoItemTemplateAsync(1, 1))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldThrowNotFound_WhenTodoItemTemplateIdDoesNotExist()
        {
            // Arrange
            var todoList = new TodoListTemplate(_currentUserId, "list1", ResetPolicy.Daily);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns(todoList);

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoItemTemplateAsync(1, 99))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task DeleteTodoItemTemplateAsync_ShouldThrowNotFound_WhenTodoListTemplateIdDoesNotExist()
        {
            // Arrange
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1))
                .Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoItemTemplateAsync(1, 99))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task AddTodoItemTemplateAsync_ShouldThrowDuplicate_WhenDuplicateDescriptionExists_CaseInsensitive()
        {
            // Arrange
            var todoListId = 1;
            var todoList = new TodoListTemplate(_currentUserId, "My List", ResetPolicy.Daily);
            todoList.AddTodoItemTemplate(new TodoItemTemplate(todoListId, new TodoItemDescription("ExiStIng tAsk")), _currentUserId);
            _repository.GetTodoListTemplateByIdAsync(Arg.Is<int>(v => v == 1), Arg.Any<bool>())
                    .Returns(todoList);

            var dto = new AddTodoItemTemplateDto { Description = "existing TASK" };

            // Act & Assert
            await _service.Invoking(s => s.AddTodoItemTemplateAsync(todoListId, dto))
                .Should().ThrowAsync<DomainDuplicateEntitiesException>();
        }
    }
}
