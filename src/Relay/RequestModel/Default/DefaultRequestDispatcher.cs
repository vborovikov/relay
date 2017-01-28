namespace Relay.RequestModel.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class DefaultRequestDispatcher : IRequestDispatcher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly MethodInfo runAsyncMethod;
        private readonly MethodInfo runMethod;

        public DefaultRequestDispatcher(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.runMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(RunInternal));
            this.runAsyncMethod = GetType().GetTypeInfo().GetDeclaredMethod(nameof(RunAsyncInternal));
        }

        public override string ToString() => "Request";

        public Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var asyncCommandHandler =
                this.serviceProvider.GetService(typeof(IAsyncCommandHandler<TCommand>)) as IAsyncCommandHandler<TCommand>;
            if (asyncCommandHandler != null)
            {
                return asyncCommandHandler.ExecuteAsync(command);
            }
            else
            {
                var commandHandler =
                    this.serviceProvider.GetService(typeof(ICommandHandler<TCommand>)) as ICommandHandler<TCommand>;
                if (commandHandler != null)
                {
                    return Task.Run(delegate { commandHandler.Execute(command); });
                }
            }

            return Task.FromResult(0);
        }

        public Task<TResult> RunAsync<TResult>(IQuery<TResult> query)
        {
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var queryHandler = this.serviceProvider.GetService(queryHandlerType);

            if (queryHandler != null)
            {
                var method = this.runMethod.MakeGenericMethod(query.GetType(), typeof(TResult));
                return Task.Run(() => (TResult)method.Invoke(this, new[] { query, queryHandler }));
            }
            else
            {
                var asyncQueryHandlerType = typeof(IAsyncQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
                var asyncQueryHandler = this.serviceProvider.GetService(asyncQueryHandlerType);

                if (asyncQueryHandler != null)
                {
                    var asyncMethod = this.runAsyncMethod.MakeGenericMethod(query.GetType(), typeof(TResult));
                    return ((Task<TResult>)asyncMethod.Invoke(this, new[] { query, asyncQueryHandler }));
                }
            }

            return Task.FromResult(default(TResult));
        }

        private Task<TResult> RunAsyncInternal<TQuery, TResult>(TQuery query, IAsyncQueryHandler<TQuery, TResult> handler) where TQuery : IQuery<TResult>
        {
            var resultTask = handler.RunAsync(query);
            return resultTask;
        }

        private TResult RunInternal<TQuery, TResult>(TQuery query, IQueryHandler<TQuery, TResult> handler) where TQuery : IQuery<TResult>
        {
            var result = handler.Run(query);
            return result;
        }
    }
}