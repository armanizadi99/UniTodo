using UniTodo.Modules.Todos.Application.DTOs;
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

            return MapPermissions(run.Permissions);
        }

        public async Task<Result<RunPermissionsDto>> UpdateRunPermissionsAsync(int runId, UpdateRunPermissionsDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);

            var permissions = new RunPermissions
            {
                MemberAllowedToCompleteUnassignedItems = dto.MemberAllowedToCompleteUnassignedItems,
                MemberAllowedToMarkIncompleteUnassignedItems = dto.MemberAllowedToMarkIncompleteUnassignedItems,
                MemberAllowedToChangeDescriptions = dto.MemberAllowedToChangeDescriptions,
                MemberAllowedToModifyNotesForUnassignedItems = dto.MemberAllowedToModifyNotesForUnassignedItems,
                MemberAllowedToAddItems = dto.MemberAllowedToAddItems,
                MemberAllowdToRemoveItems = dto.MemberAllowdToRemoveItems
            };

            var result = run.UpdatePermissions(permissions, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<RunPermissionsDto>.Failure(result.Error);

            await _unitOfWork.SaveChangesAsync();

            return MapPermissions(run.Permissions);
        }

        private static RunPermissionsDto MapPermissions(RunPermissions permissions)
        {
            return new RunPermissionsDto
            {
                MemberAllowedToCompleteUnassignedItems = permissions.MemberAllowedToCompleteUnassignedItems,
                MemberAllowedToMarkIncompleteUnassignedItems = permissions.MemberAllowedToMarkIncompleteUnassignedItems,
                MemberAllowedToChangeDescriptions = permissions.MemberAllowedToChangeDescriptions,
                MemberAllowedToModifyNotesForUnassignedItems = permissions.MemberAllowedToModifyNotesForUnassignedItems,
                MemberAllowedToAddItems = permissions.MemberAllowedToAddItems,
                MemberAllowdToRemoveItems = permissions.MemberAllowdToRemoveItems
            };
        }
    }
}
