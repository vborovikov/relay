namespace Relay.RequestModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Schedules dispatching commands to the appropriate request handlers.
    /// </summary>
    public interface IRequestScheduler : IRequestDispatcher
    {
        /// <summary>
        /// Schedules a command to be executed at a specific time.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <param name="at">The time at which the command should be executed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ScheduleAsync<TCommand>(TCommand command, DateTimeOffset at) where TCommand : ICommand;
    }
}
