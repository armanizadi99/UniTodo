using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class RunPermissionsService
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public RunPermissionsService(IRunRepository runRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RunPermissionsDto>> GetRunPermissionsAsync(int runId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);

            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();

            return run.Permissions.ToDto();
        }

        public async Task<Result<RunPermissionsDto>> UpdateRunPermissionsAsync(int runId, UpdateRunPermissionsDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);

            var permissions = new RunPermissions
            {
                MemberAllowedToCompleteUnassignedItems = dto.MemberAllowedToCompleteUnassignedItems!.Value,
                MemberAllowedToMarkIncompleteUnassignedItems = dto.MemberAllowedToMarkIncompleteUnassignedItems!.Value,
                MemberAllowedToChangeDescriptions = dto.MemberAllowedToChangeDescriptions!.Value,
                MemberAllowedToModifyNotesForUnassignedItems = dto.MemberAllowedToModifyNotesForUnassignedItems!.Value,
                MemberAllowedToAddItems = dto.MemberAllowedToAddItems!.Value,
                MemberAllowdToRemoveItems = dto.MemberAllowdToRemoveItems!.Value
            };

            var result = run.UpdatePermissions(permissions, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<RunPermissionsDto>.Failure(result.Error);

            await _unitOfWork.SaveChangesAsync();

            return run.Permissions.ToDto();
        }
    }
}
