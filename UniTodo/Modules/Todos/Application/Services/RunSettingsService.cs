using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class RunSettingsService
    {
        private readonly IRunRepository _runRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public RunSettingsService(IRunRepository runRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RunSettingsDto>> GetRunSettingsAsync(int runId, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);

            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();

            return MapSettings(run.Settings);
        }

        public async Task<Result<RunSettingsDto>> UpdateRunSettingsAsync(int runId, UpdateRunSettingsDto dto, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(runId, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), runId);

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZone);
            var settings = new RunSettings
            {
                TimeZone = timeZone,
                EndOfWeekDay = dto.EndOfWeekDay,
                PreserveHystory = dto.PreserveHystory
            };

            var result = run.UpdateSettings(settings, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<RunSettingsDto>.Failure(result.Error);

            await _unitOfWork.SaveChangesAsync();

            return MapSettings(run.Settings);
        }

        private static RunSettingsDto MapSettings(RunSettings settings)
        {
            return new RunSettingsDto
            {
                TimeZone = settings.TimeZone.Id,
                EndOfWeekDay = settings.EndOfWeekDay,
                PreserveHystory = settings.PreserveHystory
            };
        }
    }
}
