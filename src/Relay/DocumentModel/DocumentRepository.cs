namespace Relay.DocumentModel
{
    using System;

    public abstract class DocumentRepository<TDocument> : IDocumentRepository<TDocument>
        where TDocument : Document, new()
    {
        private readonly IDocumentChangeStore changeStore;

        public DocumentRepository(IDocumentChangeStore changeStore)
        {
            this.changeStore = changeStore;
        }

        public abstract void Add(TDocument document);

        public abstract void Remove(TDocument document);

        public TDocument Load(Guid documentId)
        {
            var history = this.changeStore.RestoreChanges(documentId);

            var document = new TDocument();
            document.Load(history);

            return document;
        }

        public void Save(TDocument document, int expectedVersion)
        {
            this.changeStore.StoreChanges(document.Id, document.GetRecentChanges(), expectedVersion);
            document.AcceptChanges();
        }
    }
}