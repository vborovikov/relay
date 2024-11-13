namespace Relay.Tests;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Relay.RequestModel;
using Relay.RequestModel.Default;

[TestClass]
public class SchedulerTests
{
    [TestMethod]
    public async Task ScheduleAsync_DelayFromNow_Delayed()
    {
        var cancelSource = new CancellationTokenSource();
        var scheduler = new TestCommandScheduler();
        var processTask = scheduler.ProcessAsync(cancelSource.Token);

        var command = new TestCommand { DueTime = DateTimeOffset.Now.Add(TestCommand.Delay) };
        await scheduler.ScheduleAsync(command, command.DueTime);

        await Task.Delay(TestCommand.Delay.Add(TestCommand.Lag));
        Assert.IsTrue(command.ActualTime > command.DueTime);
        var actualLag = command.ActualTime - command.DueTime;
        Assert.IsTrue(actualLag < TestCommand.Lag);
        
        cancelSource.Cancel();
    }

    [TestMethod]
    public async Task ProcessAsync_EarlyCancel_Completed()
    {
        var cancelSource = new CancellationTokenSource();
        var scheduler = new TestCommandScheduler();
        var processTask = scheduler.ProcessAsync(cancelSource.Token);

        await Task.Delay(50);
        cancelSource.Cancel();
        await Task.Delay(50);

        Assert.IsTrue(processTask.IsCompleted);
        Assert.IsTrue(processTask.IsCanceled);
    }

    private record TestCommand(CancellationToken CancellationToken = default) : ICommand
    {
        public static readonly TimeSpan Delay = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan Lag = TimeSpan.FromMilliseconds(50);

        public DateTimeOffset DueTime { get; set; }
        public DateTimeOffset ActualTime { get; set; }
    }

    private class TestCommandScheduler : DefaultRequestScheduler, ICommandHandler<TestCommand>
    {
        public TestCommandScheduler() : base(new PersistentCommandStore()) { }

        public void Execute(TestCommand command)
        {
            command.ActualTime = DateTimeOffset.Now;
        }
    }

    private record PersistentCommand(ICommand Command, DateTimeOffset DueTime) : IPersistentCommand { }

    private class PersistentCommandStore : IPersistentCommandStore
    {
        private readonly List<PersistentCommand> persistentCommands = [];

        public ValueTask AddAsync<TCommand>(TCommand command, DateTimeOffset dueTime, CancellationToken cancellationToken) where TCommand : ICommand
        {
            this.persistentCommands.Add(new(command, dueTime));
            return ValueTask.CompletedTask;
        }

        public ValueTask<IPersistentCommand?> GetAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(this.persistentCommands.MinBy(pc => pc.DueTime) as IPersistentCommand);
        }

        public ValueTask RemoveAsync(IPersistentCommand command, CancellationToken cancellationToken)
        {
            this.persistentCommands.Remove((PersistentCommand)command);
            return ValueTask.CompletedTask;
        }

        public ValueTask RetryAsync(IPersistentCommand command, Exception exception, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
}