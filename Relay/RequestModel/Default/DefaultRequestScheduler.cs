namespace Relay.RequestModel.Default
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class DefaultRequestScheduler : DefaultRequestDispatcherBase, IRequestScheduler
    {
        private readonly AsyncManualResetEvent scheduledEvent = new();
        private readonly IPersistentCommandStore commandStore;

        protected DefaultRequestScheduler(IPersistentCommandStore commandStore, TimeSpan waitPeriod = default)
        {
            this.commandStore = commandStore;
            this.WaitPeriod = waitPeriod != default ? waitPeriod : TimeSpan.FromHours(24);
        }

        public TimeSpan WaitPeriod { get; }

        /// <inheritdoc/>
        public async Task ScheduleAsync<TCommand>(TCommand command, DateTimeOffset at) where TCommand : ICommand
        {
            // store the command
            await this.commandStore.AddAsync(command, at, command.CancellationToken).ConfigureAwait(false);
            // signal for scheduling
            this.scheduledEvent.Set();
        }

        public virtual async Task ProcessAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                // get the pending command
                var pending = await this.commandStore.GetAsync(cancellationToken).ConfigureAwait(false);

                // get the wait period
                var utcNow = GetUtcNow();
                var dueTime = pending?.DueTime ?? (utcNow + this.WaitPeriod);
                var waitPeriod = dueTime - utcNow;
                if (waitPeriod > TimeSpan.Zero)
                {
                    // wait for it or a new command scheduled
                    await Task.WhenAny(Task.Delay(waitPeriod, cancellationToken), this.scheduledEvent.WaitAsync()).ConfigureAwait(false);
                    // reset the event here
                    this.scheduledEvent.Reset();

                    // start again if wait period isn't over
                    waitPeriod = dueTime - GetUtcNow();
                    if (waitPeriod > TimeSpan.Zero)
                    {
                        // time to check the store again
                        continue;
                    }
                }

                if (pending is null)
                {
                    // we waited the whole wait period, check the store again
                    continue;
                }

                try
                {
                    await this.ExecuteGenericAsync(pending.Command).ConfigureAwait(false);
                    await this.commandStore.RemoveAsync(pending, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception x) when (x is not OperationCanceledException ocx || ocx.CancellationToken != cancellationToken)
                {
                    await this.commandStore.RetryAsync(pending, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="DateTimeOffset"/> value 
        /// whose date and time are set to the current Coordinated Universal Time (UTC) date
        /// and time and whose offset is <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <returns>A <see cref="DateTimeOffset"/> value.</returns>
        protected virtual DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

        private sealed class AsyncManualResetEvent
        {
            private volatile TaskCompletionSource<bool> taskSource = new();

            public Task WaitAsync() => this.taskSource.Task;

            public void Set() => this.taskSource.TrySetResult(true);

            public void Reset()
            {
                while (true)
                {
                    var tcs = this.taskSource;
                    if (!tcs.Task.IsCompleted ||
                        Interlocked.CompareExchange(ref this.taskSource, new TaskCompletionSource<bool>(), tcs) == tcs)
                    {
                        return;
                    }
                }
            }
        }
    }

    public interface IPersistentCommand
    {
        ICommand Command { get; }
        DateTimeOffset DueTime { get; }
    }

    public interface IPersistentCommandStore
    {
        Task AddAsync<TCommand>(TCommand command, DateTimeOffset dueTime, CancellationToken cancellationToken) where TCommand : ICommand;
        Task<IPersistentCommand?> GetAsync(CancellationToken cancellationToken);
        Task RemoveAsync(IPersistentCommand command, CancellationToken cancellationToken);
        Task RetryAsync(IPersistentCommand command, CancellationToken cancellationToken);
    }
}
