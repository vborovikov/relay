namespace Relay.RequestModel
{
	using System.Threading.Tasks;

	public interface IAsyncCommandHandler<TCommand>
		where TCommand : ICommand
	{
		Task ExecuteAsync(TCommand command);
	}
}
