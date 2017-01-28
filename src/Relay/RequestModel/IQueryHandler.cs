namespace Relay.RequestModel
{
	public interface IQueryHandler<in TQuery, out TResult>
		where TQuery : IQuery<TResult>
	{
		TResult Run(TQuery query);
	}
}