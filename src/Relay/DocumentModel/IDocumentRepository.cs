namespace Relay.DocumentModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IDocumentRepository<TDocument>
        where TDocument : Document, new()
    {
        void Add(TDocument document);

        void Remove(TDocument document);

        TDocument Load(Guid documentId);

        void Save(TDocument document, int expectedVersion);
    }
}