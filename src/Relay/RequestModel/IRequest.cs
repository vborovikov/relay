namespace Relay.RequestModel
{
    using System.Threading;

    /// <summary>
    /// Defines a generic request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the cancellation token associated with the request.
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}