namespace Relay.RequestModel.Default
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base implementation of the <see cref="IRequestDispatcher"/> interface 
    /// that can dispatch requests to execute queries and commands asynchronously.
    /// </summary>
    public abstract class DefaultRequestDispatcherBase : IRequestDispatcher
    {
        private readonly MethodInfo runAsyncMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestDispatcherBase"/> class.
        /// </summary>
        protected DefaultRequestDispatcherBase()
        {
            this.runAsyncMethod = typeof(DefaultRequestDispatcherBase).GetMethod(nameof(RunInternalAsync), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <inheritdoc/>
        public override string ToString() => "Request";

        /// <summary>
        /// Executes the specified command asynchronously using a command handler that matches the type of the command.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to execute.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the specified query asynchronously and returns the result of the type <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns the result of the query.</returns>
        public Task<TResult> RunAsync<TResult>(IQuery<TResult> query)
        {
            var runAsyncGenericMethod = this.runAsyncMethod.MakeGenericMethod(query.GetType(), typeof(TResult));
            return (Task<TResult>)runAsyncGenericMethod.Invoke(this, new[] { query });
        }

        /// <summary>
        /// Gets the request handler that matches the specified type.
        /// </summary>
        /// <param name="requestHandlerType">The type of the request handler to get.</param>
        /// <returns>The request handler that matches the specified type.</returns>
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

            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Provides an implementation of the <see cref="DefaultRequestDispatcherBase"/> class
    /// that uses a dependency injection container to resolve request handlers.
    /// </summary>
    public class DefaultRequestDispatcher : DefaultRequestDispatcherBase
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRequestDispatcher"/> class
        /// with the specified <paramref name="serviceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The dependency injection container to use to resolve request handlers.</param>
        public DefaultRequestDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a request dispatcher from a handler.
        /// </summary>
        /// <param name="handler">The handler object to use to handle requests.</param>
        /// <returns>A request dispatcher that uses the specified handler.</returns>
        public static IRequestDispatcher From(object handler) => new InternalRequestDispatcher(handler);

        /// <summary>
        /// Gets the request handler that matches the specified type
        /// by resolving it from the underlying dependency injection container.
        /// </summary>
        /// <param name="requestHandlerType">The type of the request handler to get.</param>
        /// <returns>The request handler that matches the specified type.</returns>
        protected override object GetRequestHandler(Type requestHandlerType) =>
            this.serviceProvider.GetService(requestHandlerType);

        private sealed class InternalRequestDispatcher : DefaultRequestDispatcherBase
        {
            private readonly object handler;

            public InternalRequestDispatcher(object handler)
            {
                this.handler = handler;
            }

            protected override object GetRequestHandler(Type requestHandlerType) => this.handler;
        }
    }
}