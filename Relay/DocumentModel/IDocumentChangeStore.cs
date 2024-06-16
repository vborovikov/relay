namespace Relay.DocumentModel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an interface for storing and restoring document changes.
    /// </summary>
    public interface IDocumentChangeStore
    {
        /// <summary>
        /// Stores the specified document edits with the expected version number.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <param name="edits">The collection of document edits to store.</param>
        /// <param name="expectedVersion">The expected version of the document.</param>
        void StoreChanges(Guid documentId, IEnumerable<IEdit> edits, int expectedVersion);

        /// <summary>
        /// Restores the collection of document edits for the specified document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <returns>Returns the collection of document edits.</returns>
        IEnumerable<IEdit> RestoreChanges(Guid documentId);
    }
}