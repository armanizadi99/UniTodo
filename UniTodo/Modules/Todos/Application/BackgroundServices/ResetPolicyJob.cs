
using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Application.BackgroundServices
{
    public class ResetPolicyJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ResetPolicyJob> _logger;

        public ResetPolicyJob(IServiceScopeFactory scopeFactory, ILogger<ResetPolicyJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reset policy job is started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IRunRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var runsDueForReset = await repository.GetRunsDueForResetAsync(stoppingToken);
                        foreach (var run in runsDueForReset)
                        {
                            var result = run.Reset();
                            if (!result.IsSuccess)
                            {
                                _logger.LogWarning(message: "Failed to reset run {@run}. Error: {@error}", run, result.Error);
                                continue;
                            }
                            _logger.LogInformation("Reset run {@run}. New current iteration id is {IterationId}", run, run.CurrentIteration.Id);
                        }
                        await unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(exception: ex, message: "Exception thrown.");
                }
                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // app is shutting down, we'll break
                    break;
                }
            }
            _logger.LogInformation("exiting job");
        }
    }
}
