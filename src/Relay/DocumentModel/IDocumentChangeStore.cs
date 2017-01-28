namespace Relay.DocumentModel
{
    using System;
    using System.Collections.Generic;

    public interface IDocumentChangeStore
    {
        void StoreChanges(Guid documentId, IEnumerable<IEdit> edits, int expectedVersion);

        IEnumerable<IEdit> RestoreChanges(Guid documentId);
    }
}