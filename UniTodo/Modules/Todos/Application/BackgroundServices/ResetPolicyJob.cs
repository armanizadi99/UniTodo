
using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Application.BackgroundServices
{
    public class ResetPolicyJob : BackgroundService
    {
        private readonly ITodoListRunRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPolicyJob(ITodoListRunRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                await Task.Delay(1000, stoppingToken);
                var runsDueForReset = await _repository.GetRunsDueForResetAsync(stoppingToken);
                foreach (var run in runsDueForReset)
                {
                    var result = run.Reset();
                    if (result.IsSuccess)
                    {
                        await _repository.AddAsync(result.Value);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
