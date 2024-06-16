namespace Relay.RequestModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an asynchronous command handler that can execute commands of type <typeparamref name="TCommand"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of the commands to handle.</typeparam>
    public interface IAsyncCommandHandler<TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Executes the specified command asynchronously.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(TCommand command);
    }
}
