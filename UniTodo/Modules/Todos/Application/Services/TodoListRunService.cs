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

        public async Task<TodoListRunDto> CreateTodoListRunFromTemplateAsync(int templateId, CancellationToken cancellationToken)
        {
            var todoListTemplate = await _templateRepository.GetTodoListTemplateByIdOrThrowAsync(templateId, true, cancellationToken);
            if (todoListTemplate.OwnerId != _userContext.UserId)
                throw new DomainNotAuthorizedException();
            var run = TodoListRun.CreateRunFromTodoItemTemplates(todoListTemplate.TodoItemTemplates, todoListTemplate.Name, todoListTemplate.ResetPolicy, false, _userContext.UserId);
            await _runRepository.AddAsync(run);
            await _unitOfWork.SaveChangesAsync();
            return new TodoListRunDto(run.Id, run.RunId, run.Name, run.ResetPolicy, run.ownerId.Value, run.Status, run.IsShared, run.ClosedAt, run.CreatedAt, run.UpdatedAt);
        }

        public async Task<TodoListRunDto> CreateTodoListRunAsync(CreateTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = new TodoListRun(dto.Name, dto.ResetPolicy!.Value, false, _userContext.UserId);
            await _runRepository.AddAsync(run);
            await _unitOfWork.SaveChangesAsync();
            return new TodoListRunDto(run.Id, run.RunId, run.Name, run.ResetPolicy, run.ownerId.Value, run.Status, run.IsShared, run.ClosedAt, run.CreatedAt, run.UpdatedAt);
        }

        public async Task<TodoItemDto> AddTodoItemToTodoListRunAsync(int todoListRunId, AddTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, true, cancellationToken);
            if (run.ownerId != _userContext.UserId)
                throw new DomainNotAuthorizedException();
            var item = new TodoItem(new Domain.ValueObjects.TodoItemDescription(dto.Description));
            run.AddTodoItem(item, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
            return item.ToTodoItemDto();
        }

        public async Task DeleteTodoItemFromTodoListRunAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.DeleteItem(todoItemId, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<TodoItemDto>> GetTodoListRunItemsAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, true, cancellationToken);
            if (!run.Members.Any(m => m == _userContext.UserId))
                throw new DomainNotAuthorizedException();
            return run.TodoItems.Select(i => i.ToTodoItemDto()).ToList();
        }

        public async Task<IReadOnlyList<TodoListRunMemberDto>> GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, true, cancellationToken);
            if (!run.Members.Any(m => m == _userContext.UserId))
                throw new DomainNotAuthorizedException();
            return run.Members.Select(m => new TodoListRunMemberDto(m.Value)).ToList();
        }

        public async Task<IReadOnlyList<TodoListRunDto>> GetUserActiveTodoRunsAsync(CancellationToken cancellationToken)
        {
            var runs = await _runRepository.GetUserActiveRunsAsync(_userContext.UserId.Value, cancellationToken);
            return runs.Select(r => r.ToDto()).ToList();
        }

        public async Task MakeTodoListRunSharedAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(id, false, cancellationToken);
            run.MakeShared(_userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MakeTodoListRunPrivateAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(id, true, cancellationToken);
            run.MakePrivate(_userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkTodoItemCompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.MarkItemComplete(todoItemId, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkTodoItemIncompleteAsync(int todoListRunId, int todoItemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.MarkItemIncomplete(todoItemId, _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateNotesForTodoItemAsync(int todoListRunId, int todoItemId, UpdateNotesForTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.UpdateNotes(todoItemId, new Domain.ValueObjects.TodoItemNotes(dto.Notes), _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AsignMemberToItemAsync(int todoListRunId, int todoItemId, AsignMemberToTodoItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.AsignItemToMember(todoItemId, new Domain.ValueObjects.UserId(dto.MemberId!.Value), _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangeTodoItemDescriptionAsync(int todoListRunId, int todoItemId, ChangeTodoItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, todoItemId, cancellationToken);
            run.ChangeItemDescription(todoItemId, new Domain.ValueObjects.TodoItemDescription(dto.Description), _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TodoListRunMemberDto> AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, false, cancellationToken);
            var member = run.AddMember(new Domain.ValueObjects.UserId(dto.UserId!.Value), _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
            return new TodoListRunMemberDto(member);
        }

        public async Task RemoveMemberFromTodoListRunAsync(int todoListRunId, RemoveMemberFromTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdOrThrowAsync(todoListRunId, false, cancellationToken);
            run.RemoveMember(new Domain.ValueObjects.UserId(dto.UserId!.Value), _userContext.UserId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}