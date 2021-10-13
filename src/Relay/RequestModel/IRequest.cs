namespace Relay.RequestModel
{
    using System.Threading;

    public interface IRequest
    {
        CancellationToken CancellationToken { get; }
    }
}   