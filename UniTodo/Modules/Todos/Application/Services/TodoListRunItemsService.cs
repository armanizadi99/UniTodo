using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListRunItemsService
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public TodoListRunItemsService(ITodoListRunRepository runRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TodoItemDto>> AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var item = new TodoItem(new TodoItemDescription(dto.Description));
            var result = run.AddTodoItem(item, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<TodoItemDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return item.ToTodoItemDto();
        }

        public async Task<Result> DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.DeleteItem(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<TodoItemDto>>> GetTodoListRunItemsAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();
            return run.TodoItems.Select(i => i.ToTodoItemDto()).ToList();
        }

        public async Task<Result> MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.MarkItemComplete(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MarkTodoItemIncompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.MarkItemIncomplete(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.UpdateNotes(todoItemId, new TodoItemNotes(dto.Notes), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> AssignItemToMemberAsync(int todoListRunId, int todoItemId, AssignTodoItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.AssignItemToMember(todoItemId, new UserId(dto.MemberId!.Value), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.ChangeItemDescription(todoItemId, new TodoItemDescription(dto.Description), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
