using UniTodo.Modules.Todos.Application.DTOs;
using UniTodo.Modules.Todos.Application.Extensions;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Common;
using UniTodo.Modules.Todos.Domain.Entities;

namespace UniTodo.Modules.Todos.Application.Services
{
    public class RunService
    {
        private readonly IRunRepository _runRepository;
        private readonly ITodoListTemplateRepository _templateRepository;
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;

        public RunService(IRunRepository runRepository, ITodoListTemplateRepository templateRepository, IUserContext userContext, IUnitOfWork unitOfWork)
        {
            _runRepository = runRepository;
            _templateRepository = templateRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RunDto>> CreateRunFromTemplateAsync(int templateId, CancellationToken cancellationToken)
        {
            var todoListTemplate = await _templateRepository.GetTodoListTemplateByIdAsync(templateId, true, cancellationToken);
            if (todoListTemplate == null)
                return DomainError.EntityNotFound(nameof(TodoListTemplate), templateId);
            if (todoListTemplate.OwnerId != _userContext.UserId)
                return DomainError.NotAuthorized();
            var result = Run.CreateRunFromRunItemTemplates(todoListTemplate.TodoItemTemplates, todoListTemplate.Name, todoListTemplate.ResetPolicy, false, _userContext.UserId);
            if (!result.IsSuccess)
                return Result<RunDto>.Failure(result.Error);
            await _runRepository.AddAsync(result.Value);
            await _unitOfWork.SaveChangesAsync();
            return result.Value.ToDto();
        }

        public async Task<Result<RunDto>> CreateRunAsync(CreateRunDto dto, CancellationToken cancellationToken)
        {
            var run = new Run(dto.Name, dto.ResetPolicy!.Value, false, _userContext.UserId);
            await _runRepository.AddAsync(run);
            await _unitOfWork.SaveChangesAsync();
            return run.ToDto();
        }

        public async Task<Result<IReadOnlyList<RunDto>>> GetUserActiveRunsAsync(CancellationToken cancellationToken)
        {
            var runs = await _runRepository.GetUserActiveRunsAsync(_userContext.UserId.Value, cancellationToken);
            return runs.Select(r => r.ToDto()).ToList();
        }

        public async Task<Result<RunDto>> GetRunByIdAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(id, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), id);
            if (!run.Members.Any(m => m.UserId == _userContext.UserId))
                return DomainError.NotAuthorized();
            return run.ToDto();
        }

        public async Task<Result> MakeRunSharedAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(id, false, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), id);
            var result = run.MakeShared(_userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> MakeRunPrivateAsync(int id, CancellationToken cancellationToken)
        {
            var run = await _runRepository.GetRunByIdAsync(id, true, cancellationToken);
            if (run == null)
                return DomainError.EntityNotFound(nameof(Run), id);
            var result = run.MakePrivate(_userContext.UserId);
            if (!result.IsSuccess)
                return Result.Failure(result.Error);
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
