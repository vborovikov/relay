﻿namespace Relay.RequestModel.Default
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base implementation of the <see cref="IRequestScheduler"/> interface.
    /// </summary>
    public abstract class DefaultRequestScheduler : DefaultRequestDispatcherBase, IRequestScheduler
    {
        private readonly AsyncManualResetEvent scheduledEvent = new();
        private readonly IPersistentCommandStore commandStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestScheduler"/> class.
        /// </summary>
        /// <param name="commandStore">A <see cref="IPersistentCommandStore"/> instance to store commands.</param>
        /// <param name="waitPeriod">A time span to wait before checking the store again.</param>
        protected DefaultRequestScheduler(IPersistentCommandStore commandStore, TimeSpan waitPeriod = default)
        {
            this.commandStore = commandStore;
            this.WaitPeriod = waitPeriod != default ? waitPeriod : TimeSpan.FromHours(24);
        }

        /// <summary>
        /// Gets the time span to wait before checking the store again.
        /// </summary>
        public TimeSpan WaitPeriod { get; }

        /// <inheritdoc/>
        public virtual async Task ScheduleAsync<TCommand>(TCommand command, DateTimeOffset at) where TCommand : ICommand
        {
            // store the command
            await this.commandStore.AddAsync(command, at, command.CancellationToken).ConfigureAwait(false);
            // signal for scheduling
            this.scheduledEvent.Set();
        }

        /// <summary>
        /// Processes scheduled commands execution.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the processing of the schedule.</returns>
        public virtual async Task ProcessAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // get the pending command
                var pending = await this.commandStore.GetAsync(cancellationToken).ConfigureAwait(false);

                // get the wait period
                var utcNow = GetUtcNow();
                var dueTime = pending?.DueTime ?? (utcNow + this.WaitPeriod);
                var waitPeriod = dueTime - utcNow;
                if (waitPeriod > TimeSpan.Zero)
                {
                    // wait for it or a new command scheduled
                    //todo: consider add here pulse event every hour or so
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
                    await this.commandStore.RemoveAsync(pending, cancellationToken).ConfigureAwait(false);
                    ExecuteCommandTask(pending, cancellationToken);
                }
                catch (Exception x) when (x is not OperationCanceledException)
                {
                    await this.commandStore.RetryAsync(pending, x, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Observes the command task to avoid the UnobservedTaskException event to be raised.
        /// </summary>
        private void ExecuteCommandTask(IPersistentCommand pending, CancellationToken cancellationToken)
        {
            var commandTask = Task.Run(() => this.ExecuteGenericAsync(pending.Command), cancellationToken);

            // Only care about tasks that may fault (not completed) or are faulted,
            // so fast-path for SuccessfullyCompleted and Canceled tasks.
            if (!commandTask.IsCompleted || commandTask.IsFaulted)
            {
                _ = ExecuteCommandAwaited(commandTask, pending, cancellationToken);
            }
        }

        /// <summary>
        /// Executes the command task and retries on failure.
        /// </summary>
        private async ValueTask ExecuteCommandAwaited(Task commandTask, IPersistentCommand pending, CancellationToken cancellationToken)
        {
            try
            {
                await commandTask.ConfigureAwait(false);
            }
            catch (Exception x) when (x is not OperationCanceledException)
            {
                await this.commandStore.RetryAsync(pending, x, cancellationToken).ConfigureAwait(false);
                this.scheduledEvent.Set();
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

    /// <summary>
    /// Represents a persistent command.
    /// </summary>
    public interface IPersistentCommand
    {
        /// <summary>
        /// Gets the command.
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Gets the due time of the command execution.
        /// </summary>
        DateTimeOffset DueTime { get; }
    }

    /// <summary>
    /// Represents a persistent command store.
    /// </summary>
    public interface IPersistentCommandStore
    {
        /// <summary>
        /// Adds the command to the store.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="dueTime">The due time of the command execution.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask AddAsync<TCommand>(TCommand command, DateTimeOffset dueTime, CancellationToken cancellationToken) where TCommand : ICommand;

        /// <summary>
        /// Gets the next pending command.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that wraps the pending command.</returns>
        ValueTask<IPersistentCommand?> GetAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Removes the command from the store.
        /// </summary>
        /// <param name="command">The persistent command object.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        ValueTask RemoveAsync(IPersistentCommand command, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the command to the store for a retry attempt.
        /// </summary>
        /// <param name="command">The persistent command object.</param>
        /// <param name="exception">The exception that caused the retry.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>This method is called when the command execution fails. It never throws.</remarks>
        ValueTask RetryAsync(IPersistentCommand command, Exception exception, CancellationToken cancellationToken);
    }
}
