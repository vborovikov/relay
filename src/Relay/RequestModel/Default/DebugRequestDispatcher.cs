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
            var commandName = command.GetType().Name + "(" + command.GetHashCode() + ")";
            var stopwatch = new Stopwatch();
            try
            {
                Debug.WriteLine("{0} executing {1} [{2}]", this.dispatcherName, commandName, command.ToString());
                stopwatch.Start();

                await this.dispatcher.ExecuteAsync(command);
            }
            catch (Exception x)
            {
                Debug.WriteLine("{0} caught error: {1}", this.dispatcherName, x);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("{0} executed {1} ({2} ms)", this.dispatcherName,
                    commandName, stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task<TResult> RunAsync<TResult>(IQuery<TResult> query)
        {
            var queryName = query.GetType().Name + "(" + query.GetHashCode() + ")";
            var stopwatch = new Stopwatch();
            try
            {
                Debug.WriteLine("{0} running {1} [{2}]", this.dispatcherName, queryName, query.ToString());
                stopwatch.Start();

                return await this.dispatcher.RunAsync(query);
            }
            catch (Exception x)
            {
                Debug.WriteLine("{0} caught error: {1}", this.dispatcherName, x);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("{0} ran {1} ({2} ms)", this.dispatcherName,
                    queryName, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}