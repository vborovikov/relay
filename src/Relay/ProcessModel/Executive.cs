namespace Relay.ProcessModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class Executive<TState> : IDisposable
        where TState : class
    {
        // The try-again period is used in case if the Executive skips an execution, 
        // it's equal to the default Windows timer resolution.
        private static readonly TimeSpan DefaultTryAgainPeriod = TimeSpan.FromMilliseconds(15);
        private static readonly ConcurrentDictionary<TState, SemaphoreSlim> runSyncs;
        private readonly SemaphoreSlim runSync;
        private readonly Timer runTimer;
        private bool isScheduling;
        private TaskCompletionSource<bool> exeTaskSource;

        static Executive()
        {
            runSyncs = new ConcurrentDictionary<TState, SemaphoreSlim>();
        }

        protected Executive(TState state)
        {
            this.runSync = runSyncs.GetOrAdd(state, s => new SemaphoreSlim(1, 1));
            this.runTimer = new Timer(Run, state, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public bool IsExecuting { get; private set; }

        protected Task ExecutingTask => this.exeTaskSource?.Task ?? Task.CompletedTask;

        protected void Start(TimeSpan? delay = null)
        {
            Restart(delay);
        }

        private void Restart(TimeSpan? delay)
        {
            if (!this.isScheduling)
            {
                this.isScheduling = true;
                var actualDelay = delay ?? TimeSpan.Zero;
                var period = actualDelay > TimeSpan.Zero ? actualDelay : DefaultTryAgainPeriod;
                this.runTimer.Change(actualDelay, period);
            }
        }

        protected void Stop()
        {
            Pause();
            StopExecuting();
        }

        private void Pause()
        {
            this.runTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            this.isScheduling = false;
        }

        private async void Run(object state)
        {
            var hasLock = false;
            var runDelay = Timeout.InfiniteTimeSpan;
            try
            {
                hasLock = this.runSync.Wait(0);
                if (!hasLock)
                {
                    return;
                }
                Pause();
                StartExecuting();

                runDelay = await ExecuteAsync(state as TState).ConfigureAwait(false);
            }
            finally
            {
                if (hasLock)
                {
                    this.runSync.Release();
                    if (this.IsExecuting)
                    {
                        if (runDelay != Timeout.InfiniteTimeSpan && runDelay >= TimeSpan.Zero)
                        {
                            Restart(runDelay);
                        }
                        else
                        {
                            StopExecuting();
                        }
                    }
                }
            }
        }

        private void StartExecuting()
        {
            if (!this.IsExecuting)
            {
                this.IsExecuting = true;
                this.exeTaskSource = new TaskCompletionSource<bool>();
                OnStarted();
            }
        }

        protected virtual void OnStarted()
        {
        }

        private void StopExecuting()
        {
            if (this.IsExecuting)
            {
                this.IsExecuting = false;
                this.exeTaskSource.SetResult(true);
                OnStopped();
            }
        }

        protected virtual void OnStopped()
        {
        }

        protected abstract Task<TimeSpan> ExecuteAsync(TState state);

        #region IDisposable Support
        private bool isDisposed = false; // To detect redundant calls

        protected void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.runTimer.Dispose();
                    DisposeManagedObjects();
                }

                this.isDisposed = true;
            }
        }

        protected virtual void DisposeManagedObjects()
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
