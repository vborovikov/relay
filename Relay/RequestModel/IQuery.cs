namespace Relay.RequestModel
{
    /// <summary>
    /// Defines a request that returns a result without modifying the application state.
    /// </summary>
    public interface IQuery : IRequest
    {
    }

    /// <summary>
    /// Defines a request that returns the result without modifying the application state.
    /// </summary>
    public interface IQuery<TResult> : IQuery
    {
    }
}