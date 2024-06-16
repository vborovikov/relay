namespace Relay.DocumentModel
{
    using System;

    /// <summary>
    /// Represents an interface for a repository of documents of type <typeparamref name="TDocument"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of document stored in the repository.</typeparam>
    public interface IDocumentRepository<TDocument>
        where TDocument : Document, new()
    {
        /// <summary>
        /// Adds the specified <paramref name="document"/> to the repository.
        /// </summary>
        /// <param name="document">The document to add.</param>
        void Add(TDocument document);

        /// <summary>
        /// Removes the specified <paramref name="document"/> from the repository.
        /// </summary>
        /// <param name="document">The document to remove.</param>
        void Remove(TDocument document);

        /// <summary>
        /// Loads the document with the specified <paramref name="documentId"/>.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to load.</param>
        /// <returns>Returns the loaded document.</returns>
        TDocument Load(Guid documentId);

        /// <summary>
        /// Saves the specified <paramref name="document"/> with the expected version number.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="expectedVersion">The expected version of the document.</param>
        void Save(TDocument document, int expectedVersion);
    }
}