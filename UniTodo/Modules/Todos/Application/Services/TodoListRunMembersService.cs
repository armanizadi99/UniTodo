using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class TodoListRunMembersService
    {
        private readonly ITodoListRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public TodoListRunMembersService(ITodoListRunRepository runRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<TodoListRunMemberDto>>> GetTodoListRunMembersAsync(int todoListRunId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();
            return run.Members.Select(m => m.ToDto()).ToList();
        }

        public async Task<Result<TodoListRunMemberDto>> AddMemberToTodoListRunAsync(int todoListRunId, AddMemberToTodoListRunDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.AddMember(new UserId(dto.UserId!.Value), _userContext.UserId);
            if (!result.IsSuccess)
                return Result<TodoListRunMemberDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return result.Value.ToDto();
        }

        public async Task<Result> RemoveMemberFromTodoListRunAsync(int todoListRunId, Guid memberId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetTodoListRunByIdAsync(todoListRunId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(TodoListRun), todoListRunId);
            var result = run.RemoveMember(new UserId(memberId), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
