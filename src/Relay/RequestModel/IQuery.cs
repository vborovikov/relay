namespace Relay.RequestModel
{
    public interface IQuery : IRequest
    {
    }

    public interface IQuery<TResult> : IQuery
    {
    }
}