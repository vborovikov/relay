namespace Relay.RequestModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an asynchronous query handler that can execute queries of type <typeparamref name="TQuery"/>
	/// and return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of the queries to handle.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the queries.</typeparam>
    public interface IAsyncQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes the specified query asynchronously and returns the result.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>A task representing the asynchronous operation that returns the result of the query.</returns>
        Task<TResult> RunAsync(TQuery query);
    }
}