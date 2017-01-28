namespace Relay.RequestModel
{
    using System.Threading.Tasks;

    public interface IRequestDispatcher
    {
        Task<TResult> RunAsync<TResult>(IQuery<TResult> query);

        Task ExecuteAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}