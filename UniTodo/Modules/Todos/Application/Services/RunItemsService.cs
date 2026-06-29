using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class RunItemsService
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public RunItemsService(IRunRepository runRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RunItemDto>> AddRunItemToRunAsync(int runId, AddRunItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var item = new RunItem(new TodoItemDescription(dto.Description));
            var result = run.AddRunItem(item, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<RunItemDto>.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return item.ToRunItemDto();
        }

        public async Task<Result> DeleteRunItemFromRunAsync(int runId, int itemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.DeleteItem(itemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<RunItemDto>>> GetRunItemsAsync(int runId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();
            return run.CurrentIteration.RunItems.Select(i => i.ToRunItemDto()).ToList();
        }

        public async Task<Result> MarkRunItemCompleteAsync(int runId, int itemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.MarkItemComplete(itemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MarkRunItemIncompleteAsync(int runId, int itemId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.MarkItemIncomplete(itemId, _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> UpdateNotesForRunItemAsync(int runId, int itemId, UpdateNotesForRunItemDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.UpdateNotes(itemId, new TodoItemNotes(dto.Notes), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> AssignItemToMemberAsync(int runId, int itemId, AssignRunItemToMemberDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.AssignItemToMember(itemId, new UserId(dto.MemberId!.Value), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> ChangeRunItemDescriptionAsync(int runId, int itemId, ChangeRunItemDescriptionDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, itemId, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);
            var result = run.ChangeItemDescription(itemId, new TodoItemDescription(dto.Description), _userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
