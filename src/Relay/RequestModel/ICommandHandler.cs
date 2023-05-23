namespace Relay.RequestModel
{
    /// <summary>
    /// Defines a command handler that can execute commands of type <typeparamref name="TCommand"/>.
    /// </summary>
    /// <typeparam name="TCommand">The type of the commands to handle.</typeparam>
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        void Execute(TCommand command);
    }
}