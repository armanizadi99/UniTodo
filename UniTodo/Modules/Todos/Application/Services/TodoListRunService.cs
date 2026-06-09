using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListRunService
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly ITodoListTemplateRepository _templateRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public TodoListRunService(ITodoListRunRepository runRepository, ITodoListTemplateRepository templateRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _templateRepository = templateRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TodoListRunDto>> CreateTodoListRunFromTemplateAsync(int templateId, CancellationToken cancellationToken)
        {
            var todoListTemplate = await _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, cancellationToken);
if(todoListTemplate == null)
            return DomainError.EntityNotFound(nameof(TodoListTemplate), templateId);
        if (todoListTemplate.OwnerId != _userContext.UserId)
            return DomainError.NotAuthorized();
            var result = TodoListRun.CreateRunFromTodoItemTemplates(todoListTemplate.TodoItemTemplates, todoListTemplate.Name, todoListTemplate.ResetPolicy, false, _userContext.UserId);
        if (!result.IsSuccess)
            return Result<TodoListRunDto>.Failure(result.Error);
            await _runRepository.AddAsync(result.Value);
            await _unitOfWork.SaveChangesAsync();
            return result.Value.ToDto();
        }

        public async Task<Result<TodoListRunDto>> CreateTodoListRunAsync(CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = new TodoListRun(dto.Name, dto.ResetPolicy!.Value, false, _userContext.UserId);
            await _runRepository.AddAsync(run);
            await _unitOfWork.SaveChangesAsync();
            return run.ToDto();
        }

        public async Task<Result<TodoItemDto>> AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
        if (run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var item = new TodoItem(new Domain.ValueObjects.TodoItemDescription(dto.Description));
            var result = run.AddTodoItem(item, _userContext.UserId);
        if (!result.IsSuccess)
            return Result<TodoItemDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return item.ToTodoItemDto();
        }

        public async Task<Result> DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
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
if( run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        if (!run.Members.Any(m => m.UserId == _userContext.UserId))
            return DomainError.NotAuthorized();
            return run.TodoItems.Select(i => i.ToTodoItemDto()).ToList();
        }

        public async Task<Result<IReadOnlyList<TodoListRunMemberDto>>> GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
if( run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        if (!run.Members.Any(m => m.UserId == _userContext.UserId))
            return DomainError.NotAuthorized();
            return run.Members.Select(m => m.ToDto()).ToList();
        }

        public async Task<Result<IReadOnlyList<TodoListRunDto>>> GetUserActiveTodoRunsAsync(CancellationToken cancellationToken)
        {
            var runs = await _runRepository.GetUserActiveRunsAsync(_userContext.UserId.Value, cancellationToken);
            return runs.Select(r => r.ToDto()).ToList();
        }

        public async Task<Result> MakeTodoListRunSharedAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(id, false, cancellationToken);
if(run == null)
        return DomainError.EntityNotFound(nameof(TodoListRun), id);
        var result = run.MakeShared(_userContext.UserId);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
        return Result.Success();
        }

        public async Task<Result> MakeTodoListRunPrivateAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(id, true, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), id);
        var result = run.MakePrivate(_userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
return Result.Success();
        }

        public async Task<Result> MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
var result =         run.MarkItemComplete(todoItemId, _userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
return Result.Success();
        }

        public async Task<Result> MarkTodoItemIncompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result = run.MarkItemIncomplete(todoItemId, _userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
        return Result.Success();
        }

        public async Task<Result> UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result = run.UpdateNotes(todoItemId, new Domain.ValueObjects.TodoItemNotes(dto.Notes), _userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
        return Result.Success();
        }

        public async Task<Result> AssignItemToMemberAsync(int todoListRunId, int todoItemId, AssignTodoItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result = run.AssignItemToMember(todoItemId, new Domain.ValueObjects.UserId(dto.MemberId!.Value), _userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
return Result.Success();
        }

        public async Task<Result> ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, todoItemId, cancellationToken);
if(run == null)
        return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result = run.ChangeItemDescription(todoItemId, new Domain.ValueObjects.TodoItemDescription(dto.Description), _userContext.UserId);
if(!result.IsSuccess)
return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
        return Result.Success();
        }

        public async Task<Result<TodoListRunMemberDto>> AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, false, cancellationToken);
if(run == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result= run.AddMember(new Domain.ValueObjects.UserId(dto.UserId!.Value), _userContext.UserId);
        if (!result.IsSuccess)
            return Result<TodoListRunMemberDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return result.Value.ToDto();
        }

        public async Task<Result> RemoveMemberFromTodoListRunAsync(int todoListRunId, Guid memberId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
if(run  == null)
            return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
        var result = run.RemoveMember(new Domain.ValueObjects.UserId(memberId), _userContext.UserId);
        if (!result.IsSuccess)
            return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
        return Result.Success();
        }
    }
}