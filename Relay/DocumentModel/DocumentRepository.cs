namespace Relay.DocumentModel
{
    using System;

    /// <summary>
    /// Represents an abstract class for a repository of documents of type <typeparamref name="TDocument"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of document stored in the repository.</typeparam>
    public abstract class DocumentRepository<TDocument> : IDocumentRepository<TDocument>
        where TDocument : Document, new()
    {
        private readonly IDocumentChangeStore changeStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRepository{TDocument}"/> class with the specified change store.
        /// </summary>
        /// <param name="changeStore">The change store used to store and retrieve document changes.</param>
        public DocumentRepository(IDocumentChangeStore changeStore)
        {
            this.changeStore = changeStore;
        }

        /// <summary>
        /// Adds the specified <paramref name="document"/> to the repository.
        /// </summary>
        /// <param name="document">The document to add.</param>
        public abstract void Add(TDocument document);

        /// <summary>
        /// Removes the specified <paramref name="document"/> from the repository.
        /// </summary>
        /// <param name="document">The document to remove.</param>
        public abstract void Remove(TDocument document);

        /// <summary>
        /// Loads the document with the specified <paramref name="documentId"/> from the change store.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to load.</param>
        /// <returns>Returns the loaded document.</returns>
        public TDocument Load(Guid documentId)
        {
            var history = this.changeStore.RestoreChanges(documentId);

            var document = new TDocument();
            document.Load(history);

            return document;
        }

        /// <summary>
        /// Saves the specified <paramref name="document"/> to the change store with the expected version number.
        /// </summary>
        /// <param name="document">The document to save.</param>
        /// <param name="expectedVersion">The expected version of the document.</param>
        public void Save(TDocument document, int expectedVersion)
        {
            this.changeStore.StoreChanges(document.Id, document.GetRecentChanges(), expectedVersion);
            document.AcceptChanges();
        }
    }
}