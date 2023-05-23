namespace Relay.RequestModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Dispatches commands and queries to the appropriate request handlers.
    /// </summary>
    public interface IRequestDispatcher
    {
        /// <summary>
        /// Executes a query asynchronously and returns the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns the result of the query.</returns>
        Task<TResult> RunAsync<TResult>(IQuery<TResult> query);

        /// <summary>
        /// Executes a command asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to execute.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}