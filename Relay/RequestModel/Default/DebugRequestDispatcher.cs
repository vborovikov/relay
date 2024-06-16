#define DEBUG

namespace Relay.RequestModel.Default
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a decorator implementation of the <see cref="IRequestDispatcher"/> interface
    /// that outputs debug information to the console for each executed command or query.
    /// </summary>
    public class DebugRequestDispatcher : IRequestDispatcher
    {
        private readonly IRequestDispatcher dispatcher;
        private readonly string dispatcherName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugRequestDispatcher"/> class
        /// with the specified <paramref name="dispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The request dispatcher to decorate.</param>
        public DebugRequestDispatcher(IRequestDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.dispatcherName = this.dispatcher.DiscoverDispatcherName();
        }

        /// <summary>
        /// Executes the specified command asynchronously and outputs debug information to the console.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to execute.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var commandName = $"{command.GetType().Name}({command.GetHashCode()})";
            var stopwatch = new Stopwatch();
            try
            {
                Debug.WriteLine($"{this.dispatcherName} executing {commandName} [{command}]");
                stopwatch.Start();

                await this.dispatcher.ExecuteAsync(command);
            }
            catch (Exception x)
            {
                Debug.WriteLine($"{this.dispatcherName} caught error: {x}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine($"{this.dispatcherName} executed {commandName} ({stopwatch.ElapsedMilliseconds} ms)");
            }
        }

        /// <summary>
        /// Executes the specified query asynchronously and outputs debug information to the console.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns the result of the query.</returns>
        public async Task<TResult> RunAsync<TResult>(IQuery<TResult> query)
        {
            var queryName = $"{query.GetType().Name}({query.GetHashCode()})";
            var stopwatch = new Stopwatch();
            try
            {
                Debug.WriteLine($"{this.dispatcherName} running {queryName} [{query}]");
                stopwatch.Start();

                return await this.dispatcher.RunAsync(query);
            }
            catch (Exception x)
            {
                Debug.WriteLine($"{this.dispatcherName} caught error: {x}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine($"{this.dispatcherName} ran {queryName} ({stopwatch.ElapsedMilliseconds} ms)");
            }
        }
    }
}