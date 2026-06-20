namespace Relay.RequestModel
{
    using System.Text.Json.Serialization;
    using System.Threading;

    /// <summary>
    /// Defines a generic request.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// Gets the cancellation token associated with the request.
        /// </summary>
        [JsonIgnore]
        CancellationToken CancellationToken { get; }
    }
}