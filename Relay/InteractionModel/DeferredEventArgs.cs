namespace Relay.InteractionModel;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

/// <summary>
/// Provides data for events that can be deferred until a later time.
/// </summary>
public class DeferredEventArgs : EventArgs
{
    /// <summary>
    /// Provides a value to use with events that do not have event data.
    /// </summary>
    public new static DeferredEventArgs Empty => new();

    private readonly object eventDeferralLock = new();
    private EventDeferral? eventDeferral;

    /// <summary>
    /// Gets a deferral object that allows the event to be deferred.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> that represents the deferral.</returns>
    public IDisposable GetDeferral()
    {
        lock (this.eventDeferralLock)
        {
            return this.eventDeferral ??= new();
        }
    }

    /// <summary>
    /// Gets the current deferral and resets it to null.
    /// </summary>
    /// <returns>The current <see cref="EventDeferral"/> if it exists; otherwise, null.</returns>
    internal EventDeferral? GetCurrentDeferralAndReset()
    {
        lock (this.eventDeferralLock)
        {
            var eventDeferral = this.eventDeferral;
            this.eventDeferral = null;
            return eventDeferral;
        }
    }
}

/// <summary>
/// Represents a deferral for an event, allowing the event to be completed later.
/// </summary>
sealed class EventDeferral : IDisposable
{
    private readonly TaskCompletionSource<object?> taskCompletionSource = new();

    /// <summary>
    /// Releases the resources used by the <see cref="EventDeferral"/> and completes the deferral.
    /// </summary>
    public void Dispose()
    {
        Complete();
    }

    /// <summary>
    /// Completes the deferral, allowing any waiting tasks to proceed.
    /// </summary>
    public void Complete()
    {
        this.taskCompletionSource.TrySetResult(null);
    }

    /// <summary>
    /// Waits for the deferral to be completed or canceled.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the completion of the deferral.</returns>
    internal async Task WaitForCompletion(CancellationToken cancellationToken)
    {
        using (cancellationToken.Register(() => this.taskCompletionSource.TrySetCanceled()))
        {
            await this.taskCompletionSource.Task.ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Provides extension methods for invoking event handlers asynchronously.
/// </summary>
public static class EventHandlerExtensions
{
    /// <summary>
    /// Invokes the specified event handler asynchronously with the given sender and event arguments.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments, which must derive from <see cref="DeferredEventArgs"/>.</typeparam>
    /// <param name="eventHandler">The event handler to invoke.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task InvokeAsync<TEventArgs>(this EventHandler<TEventArgs>? eventHandler, object? sender, TEventArgs eventArgs)
        where TEventArgs : DeferredEventArgs
    {
        return eventHandler.InvokeAsync(sender, eventArgs, CancellationToken.None);
    }

    /// <summary>
    /// Invokes the specified event handler asynchronously with the given sender, event arguments, and cancellation token.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments, which must derive from <see cref="DeferredEventArgs"/>.</typeparam>
    /// <param name="eventHandler">The event handler to invoke.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArgs">The event data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task InvokeAsync<TEventArgs>(this EventHandler<TEventArgs>? eventHandler, object? sender, TEventArgs eventArgs, CancellationToken cancellationToken)
        where TEventArgs : DeferredEventArgs
    {
        if (eventHandler is null)
        {
            return Task.CompletedTask;
        }

        var tasks = eventHandler.GetInvocationList()
            .OfType<EventHandler<TEventArgs>>()
            .Select(invocationDelegate =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                invocationDelegate(sender, eventArgs);

                var deferral = eventArgs.GetCurrentDeferralAndReset();
                return deferral?.WaitForCompletion(cancellationToken) ?? Task.CompletedTask;
            });

        return Task.WhenAll(tasks);
    }
}
