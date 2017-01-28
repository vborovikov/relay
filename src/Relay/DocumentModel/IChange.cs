namespace Relay.DocumentModel
{
    public interface IChange<TEdit>
        where TEdit : IEdit
    {
        void Apply(TEdit e);
    }
}