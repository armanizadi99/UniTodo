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

        public async Task<Result<TodoItemDto>> AddTodoItemToTodoListRunAsync(Guid runId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var item = new TodoItem(new TodoItemDescription(dto.Description));
            var result = run.AddTodoItem(item, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<TodoItemDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return item.ToTodoItemDto();
        }

        public async Task<Result> DeleteTodoItemFromTodoListRunAsync(Guid runId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.DeleteItem(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<TodoItemDto>>> GetTodoListRunItemsAsync(Guid runId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();
            return run.TodoItems.Select(i => i.ToTodoItemDto()).ToList();
        }

        public async Task<Result> MarkTodoItemCompleteAsync(Guid runId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.MarkItemComplete(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MarkTodoItemIncompleteAsync(Guid runId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.MarkItemIncomplete(todoItemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateNotesForTodoItemAsync(Guid runId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.UpdateNotes(todoItemId, new TodoItemNotes(dto.Notes), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> AssignItemToMemberAsync(Guid runId, int todoItemId, AssignTodoItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.AssignItemToMember(todoItemId, new UserId(dto.MemberId!.Value), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> ChangeTodoItemDescriptionAsync(Guid runId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByRunIdAsync(runId, todoItemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), runId);
            var result = run.ChangeItemDescription(todoItemId, new TodoItemDescription(dto.Description), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
