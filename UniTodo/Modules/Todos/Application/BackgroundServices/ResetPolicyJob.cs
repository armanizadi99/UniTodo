
using UniTodo.Modules.Todos.Application.Interfaces;

namespace UniTodo.Modules.Todos.Application.BackgroundServices
{
    public class ResetPolicyJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ResetPolicyJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<ITodoListRunRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var runsDueForReset = await repository.GetRunsDueForResetAsync(stoppingToken);
                        foreach (var run in runsDueForReset)
                        {
                            var result = run.Reset();
                            if (result.IsSuccess)
                                await repository.AddAsync(result.Value);
                        }
                        await unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    //I later have to log, no logging support yet.
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
        }
    }
}
