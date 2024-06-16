namespace Relay.ProcessModel;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides data for the <see cref="Process.Changed"/> event.
/// </summary>
public class ProcessChangeEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessChangeEventArgs"/> class.
    /// </summary>
    /// <param name="processState">The state of the process.</param>
    public ProcessChangeEventArgs(Process.State processState)
    {
        this.ProcessState = processState;
    }

    /// <summary>
    /// Gets the state of the process.
    /// </summary>
    public Process.State ProcessState { get; }
}

/// <summary>
/// Represents a process that can execute a sequence of activities and handle exceptions and compensations.
/// </summary>
public partial class Process : Executive<Process.State>
{
    private static readonly TimeSpan RunDelay = TimeSpan.Zero;

    private readonly State state;
    private readonly IList<IActivity> activities;

    private CancellationTokenSource runCancelSource;
    private TaskCompletionSource<State> runTaskSource;
    private TaskCompletionSource<State> startTaskSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="Process"/> class.
    /// </summary>
    /// <param name="state">The initial state of the process.</param>
    /// <param name="activities">The sequence of activities that the process will execute.</param>
    private Process(State state, IList<IActivity> activities)
        : base(state)
    {
        this.state = state ?? throw new ArgumentNullException(nameof(state));
        this.activities = activities ?? throw new ArgumentNullException(nameof(activities));
    }

    /// <summary>
    /// Gets the unique identifier of the process.
    /// </summary>
    public Guid Id => this.state.ProcessId;

    /// <summary>
    /// Gets the status of the process.
    /// </summary>
    public ProcessStatus Status => this.state.ProcessStatus;

    /// <summary>
    /// Gets a value indicating whether the process is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the process has completed.
    /// </summary>
    public bool IsCompleted => this.state.ProcessStatus switch
    {
        ProcessStatus.Executed or ProcessStatus.Compensated or ProcessStatus.Aborted => true,
        _ => false,
    };

    /// <summary>
    /// Occurs when the process state changes.
    /// </summary>
    public event EventHandler<ProcessChangeEventArgs> Changed;

    /// <summary>
    /// Gets the task that represents the execution of the process.
    /// </summary>
    public Task<State> ProcessingTask => this.runTaskSource?.Task ?? Task.FromResult(this.state.Clone());

    /// <summary>
    /// Runs the process asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RunAsync(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>();
        CancellationTokenRegistration? cancelTokenReg = null;
        cancelTokenReg = cancellationToken.Register(async delegate
        {
            tcs.TrySetCanceled(cancellationToken);
            await AbortAsync();
            cancelTokenReg?.Dispose();
        });

        StartAsync().Then(delegate
        {
            this.ExecutingTask.Then(delegate
            {
                switch (this.Status)
                {
                    case ProcessStatus.Executed:
                        tcs.TrySetResult(null);
                        break;
                    case ProcessStatus.Compensated:
                        tcs.TrySetCanceled();
                        break;
                    case ProcessStatus.Aborted:
                        tcs.TrySetException(this.state.Exception);
                        break;
                }
            });
        });

        return tcs.Task;
    }

    /// <summary>
    /// Starts the process asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<State> StartAsync()
    {
        EnsureNotCompleted();
        if (this.IsExecuting)
            throw new InvalidOperationException();

        this.runCancelSource = new CancellationTokenSource();
        this.runTaskSource = new TaskCompletionSource<State>();
        this.startTaskSource = new TaskCompletionSource<State>();

        Start();

        return this.startTaskSource.Task;
    }

    /// <summary>
    /// Called when the process starts.
    /// </summary>
    protected override void OnStarted()
    {
        this.startTaskSource.TrySetResult(this.state.Clone());
    }

    /// <summary>
    /// Called when the process stops.
    /// </summary>
    protected override void OnStopped()
    {
        if (!this.runTaskSource.TrySetResult(this.state.Clone()))
        {
            this.state.ProcessStatus = ProcessStatus.Aborted;
        }

        this.runCancelSource?.Dispose();
        this.runCancelSource = null;
    }

    /// <summary>
    /// Stops the process.
    /// </summary>
    /// <returns>The process state after full stop.</returns>
    public Task<State> StopAsync()
    {
        //if (this.Status == ProcessStatus.NotStarted)
        //    throw new InvalidOperationException();

        if (this.runCancelSource != null)
        {
            this.runCancelSource.Cancel();
        }

        return this.runTaskSource.Task;
    }

    /// <summary>
    /// Aborts the process.
    /// </summary>
    /// <returns>The process state after abort.</returns>
    public Task<State> AbortAsync()
    {
        if (EnsureNotCompleted(throwOnCompleted: false))
        {
            this.runTaskSource.TrySetCanceled();
            this.runCancelSource.Cancel();
        }

        return this.ExecutingTask.Then(this.state.Clone);
    }

    private bool EnsureNotCompleted(bool throwOnCompleted = true)
    {
        if (this.IsCompleted)
        {
            if (throwOnCompleted)
                throw new InvalidOperationException();

            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    protected override Task<TimeSpan> ExecuteAsync(State state)
    {
        return ResumeAsync(this.runCancelSource.Token);
    }

    /// <inheritdoc/>
    protected override void DisposeManagedObjects()
    {
        this.runCancelSource?.Dispose();
    }

    private async Task<TimeSpan> ResumeAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Stop();
            return Timeout.InfiniteTimeSpan;
        }

        this.IsActive = true;

        if (this.state.ProcessStatus == ProcessStatus.Executing ||
            this.state.ProcessStatus == ProcessStatus.Compensating)
        {
            switch (this.state.ProcessStatus)
            {
                case ProcessStatus.Executing:
                    await ResumeExecuting(cancellationToken);
                    break;
                case ProcessStatus.Compensating:
                    await ResumeCompensating(cancellationToken);
                    break;
            }
        }

        var completed = false;
        switch (this.state.ProcessStatus)
        {
            case ProcessStatus.NotStarted:
                ResumeNotStarted();
                break;
            case ProcessStatus.Executed:
            case ProcessStatus.Compensated:
            case ProcessStatus.Aborted:
                completed = true;
                ResumeCompleted();
                break;
        }

        this.IsActive = false;
        return completed || cancellationToken.IsCancellationRequested ?
            Timeout.InfiniteTimeSpan : RunDelay;
    }

    private void ResumeNotStarted()
    {
        this.state.ProcessStatus = ProcessStatus.Executing;
        OnChanged();
    }

    private async Task ResumeExecuting(CancellationToken cancellationToken)
    {
        if (this.state.CompletedActivityCount < this.activities.Count)
        {
            try
            {
                var currentActivity = this.activities[this.state.CompletedActivityCount];
                await currentActivity.ExecuteAsync(cancellationToken);

                this.state.CompletedActivityCount++;
                OnChanged();
            }
            catch (OperationCanceledException)
            {
                Stop();
            }
            catch (Exception x)
            {
                this.state.CompensationIndex = this.state.CompletedActivityCount;
                this.state.ProcessStatus = ProcessStatus.Compensating;
                this.state.Exception = x;
                OnChanged();
                return;
            }
        }
        else
        {
            this.state.ProcessStatus = ProcessStatus.Executed;
            OnChanged();
        }
    }

    private async Task ResumeCompensating(CancellationToken cancellationToken)
    {
        if (this.state.CompensationIndex >= 0)
        {
            try
            {
                var currentActivity = this.activities[this.state.CompensationIndex];
                await currentActivity.CompensateAsync(cancellationToken);

                this.state.CompensationIndex--;
                OnChanged();
            }
            catch (OperationCanceledException)
            {
                Stop();
            }
            catch (Exception x)
            {
                this.state.ProcessStatus = ProcessStatus.Aborted;
                this.state.Exception = x;
                OnChanged();
                return;
            }
        }
        else
        {
            this.state.ProcessStatus = ProcessStatus.Compensated;
            OnChanged();
        }
    }

    private void ResumeCompleted()
    {
        Stop();
        OnChanged();
    }

    /// <summary>
    /// Called when the process state changes.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnChanged(ProcessChangeEventArgs e)
    {
        this.Changed?.Invoke(this, e);
    }

    private void OnChanged()
    {
        OnChanged(new ProcessChangeEventArgs(this.state.Clone()));
    }
}
