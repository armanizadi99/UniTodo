using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using FluentAssertions;
using System.Runtime.Serialization;
using UniTodo.Modules.Todos.Application.BackgroundServices;
using UniTodo.Modules.Todos.Application.Interfaces;
using UniTodo.Modules.Todos.Domain.Entities;
using UniTodo.Modules.Todos.Domain.ValueObjects;
using UniTodo.Modules.Todos.Infrastructure.Db.Repositories;
using NSubstitute.ExceptionExtensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute.ReceivedExtensions;

namespace UniTodo.Tests.TodoModuleTests.Application
{
    public class ResetPolicyJobTests
    {
        private readonly ITodoListRunRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResetPolicyJob> _logger;
        private readonly ResetPolicyJob _job;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceScope _scope;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserId _ownerId = new UserId(Guid.NewGuid());

        public ResetPolicyJobTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _repository = Substitute.For<ITodoListRunRepository>();
            _logger = Substitute.For<ILogger<ResetPolicyJob>>();
            _scopeFactory = Substitute.For<IServiceScopeFactory>();
            _scope = Substitute.For<IServiceScope>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _serviceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);
            _serviceProvider.GetService(typeof(ITodoListRunRepository)).Returns(_repository);
            _scope.ServiceProvider.Returns(_serviceProvider);
            _scopeFactory.CreateScope().Returns(_scope);
            _job = new ResetPolicyJob(_scopeFactory, _logger);
        }
        private void setResetsAt(TodoListRun run, DateTimeOffset? date)
        {
            var property = typeof(TodoListRun).GetProperty(nameof(TodoListRun.ResetsAt), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            property!.SetValue(run, date);
        }

        [Fact]
        public async Task StartAsync_RunsThatAreDue_ShouldResetThemAll()
        {
            // Arrange
            var dueRuns = new List<TodoListRun>();
            var run1 = new TodoListRun("run1", Modules.Todos.Domain.Enums.ResetPolicy.Daily, false, _ownerId);
            setResetsAt(run1, run1.ResetsAt?.AddDays(-1));
            var run2 = new TodoListRun("run2", Modules.Todos.Domain.Enums.ResetPolicy.Weekly, false, _ownerId);
            setResetsAt(run2, run2.ResetsAt?.AddDays(-10));
            dueRuns.AddRange(run1, run2);
            _repository.GetRunsDueForResetAsync(Arg.Any<CancellationToken>()).Returns(dueRuns);
            using var cts = new CancellationTokenSource();

            // Act
            await _job.StartAsync(cts.Token);
            await Task.Delay(100);
            cts.Cancel();

            // Assert
            run1.Status.Should().Be(Modules.Todos.Domain.Enums.TodoListRunStatus.Closed);
            run2.Status.Should().Be(Modules.Todos.Domain.Enums.TodoListRunStatus.Closed);
            await _repository.Received(1).AddAsync(Arg.Is<TodoListRun>(r => r.Name == run1.Name));
            await _repository.Received(1).AddAsync(Arg.Is<TodoListRun>(r => r.Name == run2.Name));
            await _unitOfWork.Received(1).SaveChangesAsync();
            _logger.Received(2).Log(LogLevel.Information,
    Arg.Any<EventId>(),
    Arg.Is<object>(state => state.ToString()!.Contains("Reset run")),
    Arg.Any<Exception>(),
    Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Fact]
        public async Task StartAsync_Exception_ShouldCatchAndContinue()
        {
            // Arrange
            _repository.GetRunsDueForResetAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new DBConcurrencyException("mocked exception"));
            using var cts = new CancellationTokenSource();

            // Act
            await _job.StartAsync(cts.Token);
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            // Assert
            try
            {
                _job.ExecuteTask.Should().NotBeNull();
                _job.ExecuteTask!.IsFaulted.Should().BeFalse();
                _job.ExecuteTask!.IsCompleted.Should().BeFalse();
                _logger.Received(1).Log(
        LogLevel.Warning,
        Arg.Any<EventId>(),
        Arg.Is<object>(state => state.ToString()!.Contains("Exception thrown")),
        Arg.Any<Exception>(),
        Arg.Any<Func<object, Exception?, string>>());
            }
            finally
            {
                cts.Cancel();
            }
        }
    }
}
