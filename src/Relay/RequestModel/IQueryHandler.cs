namespace Relay.RequestModel
{
    /// <summary>
    /// Defines a query handler that can execute queries of type <typeparamref name="TQuery"/>
	/// and return a result of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TQuery">The type of the queries to handle.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the queries.</typeparam>
    public interface IQueryHandler<in TQuery, out TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes the specified query and returns the result.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        TResult Run(TQuery query);
    }
}