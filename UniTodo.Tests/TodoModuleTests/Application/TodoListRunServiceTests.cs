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
        private void SetId<T>(T entity, int id) where T : EntityBase<int>
        {
            typeof(EntityBase<int>).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, id);
        }

        private void SetStatus(TodoListRun run, TodoListRunStatus status)
        {
            typeof(TodoListRun).GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(run, status);
        }

        private void SetRun(TodoItem item, TodoListRun run)
        {
            typeof(TodoItem).GetField("<Run>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(item, run);
        }

        private TodoListRun CreateActiveRun(string name = "Test Run", bool isShared = false, UserId? ownerId = null)
        {
            return new TodoListRun(name, ResetPolicy.Daily, isShared, ownerId ?? _currentUserId);
        }
        #endregion

        #region CreateTodoListRunFromTemplateAsync
        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenUserIsOwner_ShouldCreateRunAndReturnMappedDto()
        {
            // Arrange
            var templateId = 1;
            var template = new TodoListTemplate(_currentUserId, "Template Name", ResetPolicy.Daily);
            template.AddTodoItemTemplate(new TodoItemTemplate(1, new TodoItemDescription("Item 1")), _currentUserId);

            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act
            var result = await _service.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(template.Name);
            result.ResetPolicy.Should().Be(template.ResetPolicy);
            result.OwnerId.Should().Be(_currentUserId.Value);
            result.Status.Should().Be(TodoListRunStatus.Active);
            await _runRepository.Received(1).AddAsync(Arg.Any<TodoListRun>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenUserIsNotOwner_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var templateId = 1;
            var otherUser = new UserId(Guid.NewGuid());
            var template = new TodoListTemplate(otherUser, "Other Template", ResetPolicy.Daily);
            _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, Arg.Any<CancellationToken>()).Returns(template);

            // Act & Assert
            await _service.Invoking(s => s.CreateTodoListRunFromTemplateAsync(templateId, CancellationToken.None))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task CreateTodoListRunFromTemplateAsync_WhenTemplateNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _templateRepository.GetTodoListTemplateByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListTemplate)null!);

            // Act & Assert
            await _service.Invoking(s => s.CreateTodoListRunFromTemplateAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }
        #endregion

        #region CreateTodoListRunAsync
        [Fact]
        public async Task CreateTodoListRunAsync_WhenValidRequest_ShouldCreatePrivateRunAndReturnDto()
        {
            // Arrange
            var dto = new CreateTodoListRunDto { Name = "New Run", ResetPolicy = ResetPolicy.Weekly };

            // Act
            var result = await _service.CreateTodoListRunAsync(dto, CancellationToken.None);

            // Assert
            result.Name.Should().Be(dto.Name);
            result.ResetPolicy.Should().Be(dto.ResetPolicy);
            result.IsShared.Should().BeFalse();
            await _runRepository.Received(1).AddAsync(Arg.Any<TodoListRun>());
            await _unitOfWork.Received(1).SaveChangesAsync();
        }
        #endregion

        #region AddTodoItemToTodoListRunAsync
        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenAuthorizedAndActive_ShouldAddItemAndReturnDto()
        {
            // Arrange
            var run = CreateActiveRun();
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);
            var dto = new AddTodoItemDto { Description = "New Item" };

            // Act
            var result = await _service.AddTodoItemToTodoListRunAsync(1, dto, CancellationToken.None);

            // Assert
            result.Description.Should().Be(dto.Description);
            run.TodoItems.Should().Contain(i => i.Description.Value == dto.Description);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.AddTodoItemToTodoListRunAsync(1, new AddTodoItemDto { Description = "X" }, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task AddTodoItemToTodoListRunAsync_WhenRunIsClosed_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.AddTodoItemToTodoListRunAsync(1, new AddTodoItemDto { Description = "X" }, CancellationToken.None))
                .Should().ThrowAsync<DomainInvalidOperationException>()
    .WithMessage("Items couldn't be added to a closed run.");
        }
        #endregion

        #region DeleteTodoItemFromTodoListRunAsync
        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenAuthorizedAndExists_ShouldRemoveItemAndSave()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None);

            // Assert
            run.TodoItems.Should().NotContain(item);
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task DeleteTodoItemFromTodoListRunAsync_WhenItemCorrelationFails_ShouldPropagateDomainEntityNotFoundException()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act & Assert
            await _service.Invoking(s => s.DeleteTodoItemFromTodoListRunAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>()
                .WithMessage($"*'{nameof(TodoItem)}'*'10'*");
        }
        #endregion

        #region GetTodoListRunItemsAsync
        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenUserIsMember_ShouldReturnMappedItems()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true, ownerId: new UserId(Guid.NewGuid()));
            run.AddMember(_currentUserId, run.ownerId);
            run.AddTodoItem(new TodoItem(new TodoItemDescription("description")), run.ownerId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunItemsAsync(1, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1)
            .And.Contain(i => i.Description == "description");
            await _runRepository.Received(1).GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenUserIsNotMember_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListRunItemsAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task GetTodoListRunItemsAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListRunItemsAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }
        #endregion

        #region GetTodoListRunMembersAsync
        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenUserIsMember_ShouldReturnMappedMembers()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var otherUserId = new UserId(Guid.NewGuid());
            run.AddMember(otherUserId, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.GetTodoListRunMembersAsync(1, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2)
    .And.Contain(m => m.UserId == _currentUserId.Value)
    .And.Contain(m => m.UserId == otherUserId.Value);
        }

        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenUserIsNotMember_ShouldThrowDomainNotAuthorizedException()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false, ownerId: new UserId(Guid.NewGuid()));
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListRunMembersAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainNotAuthorizedException>();
        }

        [Fact]
        public async Task GetTodoListRunMembersAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.GetTodoListRunMembersAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }
        #endregion

        #region GetUserActiveTodoRunsAsync
        [Fact]
        public async Task GetUserActiveTodoRunsAsync_WhenCalled_ShouldReturnActiveRunsForCurrentUser()
        {
            // Arrange
            var runs = new List<TodoListRun> { CreateActiveRun("Run 1") };
            _runRepository.GetUserActiveRunsAsync(_currentUserId.Value, Arg.Any<CancellationToken>()).Returns(runs);

            // Act
            var result = await _service.GetUserActiveTodoRunsAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Run 1");
        }
        #endregion

        #region MakeTodoListRunSharedAsync
        [Fact]
        public async Task MakeTodoListRunSharedAsync_WhenAuthorizedAndValid_ShouldUpdateAndSave()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.MakeTodoListRunSharedAsync(1, CancellationToken.None);

            // Assert
            run.IsShared.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeTodoListRunSharedAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.MakeTodoListRunSharedAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }
        [Fact]
        public async Task MakeTodoListRunSharedAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act Assert
            await _service.Invoking(s => s.MakeTodoListRunSharedAsync(1, CancellationToken.None))
    .Should().ThrowAsync<DomainInvalidOperationException>()
    .WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region MakeTodoListRunPrivateAsync
        [Fact]
        public async Task MakeTodoListRunPrivateAsync_WhenAuthorizedAndValid_ShouldUpdateAndSave()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.MakeTodoListRunPrivateAsync(1, CancellationToken.None);

            // Assert
            run.IsShared.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MakeTodoListRunPrivateAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.MakeTodoListRunPrivateAsync(1, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task MakeTodoListRunPrivateAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun(isShared: false);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act Assert
            await _service.Invoking(s => s.MakeTodoListRunPrivateAsync(1, CancellationToken.None))
            .Should().ThrowAsync<DomainInvalidOperationException>()
            .WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region MarkTodoItemCompleteAsync
        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenAuthorizedAndValid_ShouldMarkCompleteAndSave()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None);

            // Assert
            item.IsCompleted.Should().BeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task MarkTodoItemCompleteAsync_WhenRunIsClosed_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.MarkTodoItemCompleteAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainInvalidOperationException>()
.WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region MarkTodoItemIncompleteAsync
        [Fact]
        public async Task MarkTodoItemIncompleteAsync_WhenAuthorizedAndValid_ShouldMarkIncompleteAndSave()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            item.MarkComplete(_currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None);

            // Assert
            item.IsCompleted.Should().BeFalse();
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task MarkTodoItemIncompleteAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task MarkTodoItemIncompleteAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var runWithoutItem = CreateActiveRun();
            SetStatus(runWithoutItem, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(runWithoutItem);

            // Act & Assert
            await _service.Invoking(s => s.MarkTodoItemIncompleteAsync(1, 10, CancellationToken.None))
                .Should().ThrowAsync<DomainInvalidOperationException>()
.WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region UpdateNotesForTodoItemAsync
        [Fact]
        public async Task UpdateNotesForTodoItemAsync_WhenAuthorizedAndValid_ShouldUpdateNotesAndSave()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            SetRun(item, run);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "New Notes" }, CancellationToken.None);

            // Assert
            item.Notes?.Value.Should().Be("New Notes");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateNotesForTodoItemAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "X" }, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task UpdateNotesForTodoItemAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act Assert 
            await _service.Invoking(s => s.UpdateNotesForTodoItemAsync(1, 10, new UpdateNotesForTodoItemDto { Notes = "new notes" }, CancellationToken.None))
            .Should().ThrowAsync<DomainInvalidOperationException>()
            .WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region AsignMemberToItemAsync
        [Fact]
        public async Task AsignMemberToItemAsync_WhenAuthorizedAndValid_ShouldAssignAndSave()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = memberId }, CancellationToken.None);

            // Assert
            item.AssignedTo.Should().Be(new UserId(memberId));
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AsignMemberToItemAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task AsignMemberToItemAsync_WhenMemberNotInRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var item = new TodoItem(new TodoItemDescription("Item"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.AssignItemToMemberAsync(1, 10, new AssignTodoItemToMemberDto { MemberId = Guid.NewGuid() }, CancellationToken.None))
                .Should().ThrowAsync<DomainInvalidOperationException>()
    .WithMessage("An item couldn't get asigned to someone that is not a member of the run.");
        }
        #endregion

        #region ChangeTodoItemDescriptionAsync
        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_WhenAuthorizedAndValid_ShouldUpdateAndSave()
        {
            // Arrange
            var run = CreateActiveRun();
            var item = new TodoItem(new TodoItemDescription("Old"));
            SetId(item, 10);
            run.AddTodoItem(item, _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "New" }, CancellationToken.None);

            // Assert
            item.Description.Value.Should().Be("New");
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "X" }, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task ChangeTodoItemDescriptionAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, 10, Arg.Any<CancellationToken>()).Returns(run);

            // Act Assert
            await _service.Invoking(s => s.ChangeTodoItemDescriptionAsync(1, 10, new ChangeTodoItemDescriptionDto { Description = "new desc" }, CancellationToken.None))
            .Should().ThrowAsync<DomainInvalidOperationException>()
            .WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region AddMemberToTodoListRunAsync
        [Fact]
        public async Task AddMemberToTodoListRunAsync_WhenAuthorizedAndValid_ShouldAddAndReturnDto()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var newMemberId = Guid.NewGuid();
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            var result = await _service.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = newMemberId }, CancellationToken.None);

            // Assert
            result.UserId.Should().Be(newMemberId);
            run.Members.Should().Contain(new UserId(newMemberId));
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddMemberToTodoListRunAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = Guid.NewGuid() }, CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task AddMemberToTodoListRunAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, false, Arg.Any<CancellationToken>()).Returns(run);

            // Act & Assert
            await _service.Invoking(s => s.AddMemberToTodoListRunAsync(1, new AddMemberToTodoListRunDto { UserId = Guid.NewGuid() }, CancellationToken.None))
                .Should().ThrowAsync<DomainInvalidOperationException>()
    .WithMessage("A closed run couldn't get modified.");
        }
        #endregion

        #region RemoveMemberFromTodoListRunAsync
        [Fact]
        public async Task RemoveMemberFromTodoListRunAsync_WhenAuthorizedAndValid_ShouldRemoveAndSave()
        {
            // Arrange
            var run = CreateActiveRun(isShared: true);
            var memberId = Guid.NewGuid();
            run.AddMember(new UserId(memberId), _currentUserId);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act
            await _service.RemoveMemberFromTodoListRunAsync(1, memberId, CancellationToken.None);

            // Assert
            run.Members.Should().NotContain(new UserId(memberId));
            await _unitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveMemberFromTodoListRunAsync_WhenRunNotFound_ShouldThrowDomainEntityNotFoundException()
        {
            // Arrange
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns((TodoListRun)null!);

            // Act & Assert
            await _service.Invoking(s => s.RemoveMemberFromTodoListRunAsync(1, Guid.NewGuid(), CancellationToken.None))
                .Should().ThrowAsync<DomainEntityNotFoundException>();
        }

        [Fact]
        public async Task RemoveMemberToTodoListRunAsync_ClosedRun_ShouldPropagateDomainInvalidOperationException()
        {
            // Arrange
            var run = CreateActiveRun();
            SetStatus(run, TodoListRunStatus.Closed);
            _runRepository.GetTodoListRunByIdAsync(1, true, Arg.Any<CancellationToken>()).Returns(run);

            // Act Assert
            await _service.Invoking(s => s.RemoveMemberFromTodoListRunAsync(1, Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<DomainInvalidOperationException>()
            .WithMessage("A closed run couldn't get modified.");
        }
        #endregion
    }
}
