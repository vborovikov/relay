namespace Relay.ProcessModel;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a logical unit of work that can be executed by a process.
/// </summary>
public interface IActivity
{
    /// <summary>
    /// Executes the activity.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Compensates the activity.
    /// </summary>
    Task CompensateAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Represents a logical unit of work that can be executed by a process.
/// </summary>
abstract class Activity : IActivity
{
    /// <inheritdoc />
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public abstract Task CompensateAsync(CancellationToken cancellationToken);
}

/// <inheritdoc />
abstract class Activity<TContext> : Activity
{
    protected Activity(TContext context)
    {
        this.Context = context;
    }

    /// <summary>
    /// Gets the context that is passed to the activity.
    /// </summary>
    public TContext Context { get; }
}

/// <summary>
/// Wraps a function that is called when an activity is executed.
/// </summary>
class DelegateActivity<TContext> : Activity<TContext>
{
    private readonly Func<TContext, CancellationToken, Task> execute;
    private readonly Func<TContext, CancellationToken, Task> compensate;

    public DelegateActivity(TContext context,
        Func<TContext, CancellationToken, Task> execute,
        Func<TContext, CancellationToken, Task> compensate)
        : base(context)
    {
        this.execute = execute;
        this.compensate = compensate;
    }

    /// <inheritdoc />
    public override Task ExecuteAsync(CancellationToken cancellationToken) =>
        this.execute(this.Context, cancellationToken);

    /// <inheritdoc />
    public override Task CompensateAsync(CancellationToken cancellationToken) =>
        this.compensate(this.Context, cancellationToken);
}