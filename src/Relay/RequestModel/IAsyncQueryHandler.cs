namespace Relay.RequestModel
{
	using System.Threading.Tasks;

	public interface IAsyncQueryHandler<in TQuery, TResult>
		where TQuery : IQuery<TResult>
	{
		Task<TResult> RunAsync(TQuery query);
	}
}