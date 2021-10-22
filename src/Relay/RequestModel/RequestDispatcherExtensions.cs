namespace Relay.RequestModel
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

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

        public static Task ExecuteNonGenericAsync(this IRequestDispatcher dispatcher, ICommand command)
        {
            var method = executeAsyncMethod.MakeGenericMethod(command.GetType());
            return (Task)method.Invoke(dispatcher, new[] { command });
        }

        public static async Task<object> RunNonGenericAsync(this IRequestDispatcher dispatcher, IRequest query)
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