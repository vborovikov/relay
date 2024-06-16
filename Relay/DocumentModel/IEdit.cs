namespace Relay.DocumentModel
{
    using System;

    /// <summary>
    /// Represents an single edit to a document.
    /// </summary>
    public interface IEdit
    {
        /// <summary>
        /// Gets the unique identifier of the document being edited.
        /// </summary>
        Guid DocumentId { get; }
    }
}