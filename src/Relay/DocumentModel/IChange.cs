namespace Relay.DocumentModel
{
    /// <summary>
    /// Represents an interface for applying changes to a document.
    /// </summary>
    /// <typeparam name="TEdit">The type of edit being applied.</typeparam>
    public interface IChange<TEdit>
        where TEdit : IEdit
    {
        /// <summary>
        /// Applies the specified edit to the document.
        /// </summary>
        /// <param name="e">The edit to apply.</param>
        void Apply(TEdit e);
    }
}