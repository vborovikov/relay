namespace Relay.RequestModel.Default
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class DefaultRequestDispatcherBase : IRequestDispatcher
    {
        private readonly MethodInfo runAsyncMethod;

        protected DefaultRequestDispatcherBase()
        {
            this.runAsyncMethod = typeof(DefaultRequestDispatcherBase).GetMethod(nameof(RunInternalAsync), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public override string ToString() => "Request";

        public Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var asyncCommandHandler = GetRequestHandler(typeof(IAsyncCommandHandler<TCommand>)) as IAsyncCommandHandler<TCommand>;
            if (asyncCommandHandler != null)
            {
                return asyncCommandHandler.ExecuteAsync(command);
            }
            else
            {
                var commandHandler = GetRequestHandler(typeof(ICommandHandler<TCommand>)) as ICommandHandler<TCommand>;
                if (commandHandler != null)
                {
                    return Task.Run(delegate { commandHandler.Execute(command); });
                }
            }

            return Task.CompletedTask;
        }

        public Task<TResult> RunAsync<TResult>(IQuery<TResult> query)
        {
            var runAsyncGenericMethod = this.runAsyncMethod.MakeGenericMethod(query.GetType(), typeof(TResult));
            return (Task<TResult>)runAsyncGenericMethod.Invoke(this, new[] { query });
        }

        protected virtual object GetRequestHandler(Type requestHandlerType) => this;

        private Task<TResult> RunInternalAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            var asyncQueryHandler = GetRequestHandler(typeof(IAsyncQueryHandler<TQuery, TResult>)) as IAsyncQueryHandler<TQuery, TResult>;
            if (asyncQueryHandler != null)
            {
                return asyncQueryHandler.RunAsync(query);
            }
            else
            {
                var queryHandler = GetRequestHandler(typeof(IQueryHandler<TQuery, TResult>)) as IQueryHandler<TQuery, TResult>;
                if (queryHandler != null)
                {
                    return Task.Run(() => queryHandler.Run(query));
                }
            }

            return Task.FromResult(default(TResult));
        }
    }

    public class DefaultRequestDispatcher : DefaultRequestDispatcherBase
    {
        private readonly IServiceProvider serviceProvider;

        public DefaultRequestDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override object GetRequestHandler(Type requestHandlerType) =>
            this.serviceProvider.GetService(requestHandlerType);
    }
}