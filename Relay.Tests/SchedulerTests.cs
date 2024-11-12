namespace Relay.Tests;

using System.Threading;
using System.Threading.Tasks;
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

        public Task AddAsync<TCommand>(TCommand command, DateTimeOffset dueTime, CancellationToken cancellationToken) where TCommand : ICommand
        {
            this.persistentCommands.Add(new(command, dueTime));
            return Task.CompletedTask;
        }

        public Task<IPersistentCommand?> GetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.persistentCommands.MinBy(pc => pc.DueTime) as IPersistentCommand);
        }

        public Task RemoveAsync(IPersistentCommand command, CancellationToken cancellationToken)
        {
            this.persistentCommands.Remove((PersistentCommand)command);
            return Task.CompletedTask;
        }

        public Task RetryAsync(IPersistentCommand command, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}