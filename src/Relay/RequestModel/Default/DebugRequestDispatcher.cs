#define DEBUG

namespace Relay.RequestModel.Default
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class DebugRequestDispatcher : IRequestDispatcher
    {
        private readonly IRequestDispatcher dispatcher;
        private readonly string dispatcherName;

        public DebugRequestDispatcher(IRequestDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.dispatcherName = this.dispatcher.DiscoverDispatcherName();
        }

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