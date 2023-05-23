namespace Relay.RequestModel
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="IRequestDispatcher"/> interface.
    /// </summary>
    public static class RequestDispatcherExtensions
    {
        private static readonly MethodInfo executeAsyncMethod =
            typeof(IRequestDispatcher).GetMethod(nameof(IRequestDispatcher.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance);
        private static readonly MethodInfo runAsyncMethod =
            typeof(IRequestDispatcher).GetMethod(nameof(IRequestDispatcher.RunAsync), BindingFlags.Public | BindingFlags.Instance);

        internal static string DiscoverDispatcherName(this IRequestDispatcher dispatcher)
        {
            var name = dispatcher.ToString();

            if (name == dispatcher.GetType().FullName)
            {
                name = dispatcher.GetType().Name;
            }

            return name;
        }

        /// <summary>
        /// Executes a generic command asynchronously using the specified request dispatcher.
        /// </summary>
        /// <param name="dispatcher">The request dispatcher to use.</param>
        /// <param name="command">The generic command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task ExecuteGenericAsync(this IRequestDispatcher dispatcher, ICommand command)
        {
            var method = executeAsyncMethod.MakeGenericMethod(command.GetType());
            return (Task)method.Invoke(dispatcher, new[] { command });
        }

        /// <summary>
        /// Runs a generic query asynchronously using the specified request dispatcher and returns the result as an object.
        /// </summary>
        /// <param name="dispatcher">The request dispatcher to use.</param>
        /// <param name="query">The generic query to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns the result of the query as an object.</returns>
        public static async Task<object> RunGenericAsync(this IRequestDispatcher dispatcher, IQuery query)
        {
            var method = runAsyncMethod.MakeGenericMethod(FindGenericArgument(query.GetType()));
            var queryTask = (Task)method.Invoke(dispatcher, new[] { query });
            await queryTask;
            var result = queryTask
                .GetType()
                .GetProperty(nameof(Task<object>.Result))
                .GetGetMethod()
                .Invoke(queryTask, Array.Empty<object>());
            return result;
        }

        private static Type FindGenericArgument(Type type)
        {
            var queryIfaceDef = typeof(IQuery<>);
            var queryType = Array.Find(type.GetInterfaces(),
                iface => iface.IsGenericType && iface.GetGenericTypeDefinition().Equals(queryIfaceDef));

            return queryType.GetGenericArguments()[0];
        }
    }
}