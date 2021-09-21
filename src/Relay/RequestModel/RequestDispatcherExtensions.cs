namespace Relay.RequestModel
{
    using System.Reflection;
    using System.Threading.Tasks;

    public static class RequestDispatcherExtensions
    {
        private static readonly MethodInfo executeAsyncMethod =
            typeof(IRequestDispatcher).GetMethod(nameof(IRequestDispatcher.ExecuteAsync), BindingFlags.Public | BindingFlags.Instance);

        public static Task ExecuteNonGenericAsync(this IRequestDispatcher dispatcher, ICommand command)
        {
            var method = executeAsyncMethod.MakeGenericMethod(command.GetType());
            return (Task)method.Invoke(dispatcher, new[] { command });
        }

        internal static string DiscoverDispatcherName(this IRequestDispatcher dispatcher)
        {
            var name = dispatcher.ToString();

            if (name == dispatcher.GetType().FullName)
            {
                name = dispatcher.GetType().Name;
            }

            return name;
        }
    }
}