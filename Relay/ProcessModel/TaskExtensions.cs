namespace Relay.ProcessModel;

using System;
using System.Threading.Tasks;

static class TaskExtensions
{
    /// <summary>Creates a task that represents the completion of a follow-up action when a task completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="next">The action to run when the task completes.</param>
    /// <returns>The task that represents the completion of both the task and the action.</returns>
    public static Task Then(this Task task, Action next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<object?>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    next();
                    tcs.TrySetResult(null);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }

    /// <summary>Creates a task that represents the completion of a follow-up function when a task completes.</summary>
    /// <param name="task">The task.</param>
    /// <param name="next">The function to run when the task completes.</param>
    /// <returns>The task that represents the completion of both the task and the function.</returns>
    public static Task<TResult> Then<TResult>(this Task task, Func<TResult> next)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        if (next == null) throw new ArgumentNullException(nameof(next));

        var tcs = new TaskCompletionSource<TResult>();
        task.ContinueWith(delegate
        {
            if (task.IsFaulted) tcs.TrySetException(task.Exception.InnerExceptions);
            else if (task.IsCanceled) tcs.TrySetCanceled();
            else
            {
                try
                {
                    var result = next();
                    tcs.TrySetResult(result);
                }
                catch (Exception exc) { tcs.TrySetException(exc); }
            }
        }, TaskScheduler.Default);
        return tcs.Task;
    }
}
