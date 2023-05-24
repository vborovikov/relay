namespace Relay.DocumentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    /// Represents a document that implements <see cref="IRevertibleChangeTracking"/>.
    /// </summary>
    public abstract class Document : IRevertibleChangeTracking
    {
        /// <summary>
        /// The value that represents no version number.
        /// </summary>
        public const int NoVersion = -1;

        private static readonly MethodInfo applyChangeMethod =
            typeof(Document).GetMethod(nameof(ApplyChangeInternal), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly List<IEdit> changes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        protected Document()
        {
            this.changes = new List<IEdit>();
            this.Version = NoVersion;
            this.IsActive = true;
        }

        /// <summary>
        /// Gets the unique identifier of the document.
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        /// Gets the version of the document.
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the document is active.
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the document has been changed.
        /// </summary>
        public bool IsChanged { get { return this.changes.Any(); } }

        /// <summary>
        /// Loads the document with the specified collection of history edits.
        /// </summary>
        /// <param name="history">The collection of history edits.</param>
        public void Load(IEnumerable<IEdit> history)
        {
            foreach (var @event in history)
            {
                ApplyChange(@event, fromHistory: true);
            }
        }

        /// <summary>
        /// Gets the collection of recent changes made to the document.
        /// </summary>
        /// <returns>Returns the collection of recent changes.</returns>
        public IEnumerable<IEdit> GetRecentChanges()
        {
            return this.changes.ToArray();
        }

        /// <summary>
        /// Accepts all changes made to the document.
        /// </summary>
        public void AcceptChanges()
        {
            this.Version += this.changes.Count;
            this.changes.Clear();
        }

        /// <summary>
        /// Rejects all changes made to the document.
        /// </summary>
        public void RejectChanges()
        {
            this.changes.Clear();
        }

        /// <summary>
        /// Applies the specified edit to the document.
        /// </summary>
        /// <param name="edit">The edit to apply.</param>
        protected void ApplyChange(IEdit edit)
        {
            ApplyChange(edit, fromHistory: false);
        }

        private static void ApplyChangeInternal<TEdit>(TEdit edit, Document that)
            where TEdit : IEdit
        {
            var change = that as IChange<TEdit>;
            if (change == null)
            {
                throw new NotImplementedException();
            }

            change.Apply(edit);
        }

        private void ApplyChange(IEdit edit, bool fromHistory)
        {
            try
            {
                var applyChange = applyChangeMethod.MakeGenericMethod(edit.GetType());
                applyChange.Invoke(null, new object[] { edit, this });

                if (fromHistory == false)
                {
                    this.changes.Add(edit);
                }
                else
                {
                    this.Version += 1;
                }
            }
            catch (TargetInvocationException tix)
            {
                if (tix.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(tix.InnerException).Throw();
                }
            }
        }
    }
}