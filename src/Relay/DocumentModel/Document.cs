namespace Relay.DocumentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    public abstract class Document : IRevertibleChangeTracking
    {
        public const int NoVersion = -1;

        private static readonly MethodInfo applyChangeMethod =
#if PORTABLE
            typeof(Document).GetTypeInfo().DeclaredMethods.Single(m => m.Name == nameof(ApplyChangeInternal) && m.IsPublic == false && m.IsStatic);

#else
            typeof(Document).GetMethod(nameof(ApplyChangeInternal), BindingFlags.NonPublic | BindingFlags.Static);

#endif

        private readonly List<IEdit> changes;

        protected Document()
        {
            this.changes = new List<IEdit>();
            this.Version = NoVersion;
            this.IsActive = true;
        }

        public abstract Guid Id { get; }

        public int Version { get; private set; }

        public bool IsActive { get; protected set; }

        public bool IsChanged { get { return this.changes.Any(); } }

        public void Load(IEnumerable<IEdit> history)
        {
            foreach (var @event in history)
            {
                ApplyChange(@event, fromHistory: true);
            }
        }

        public IEnumerable<IEdit> GetRecentChanges()
        {
            return this.changes.ToArray();
        }

        public void AcceptChanges()
        {
            this.Version += this.changes.Count;
            this.changes.Clear();
        }

        public void RejectChanges()
        {
            this.changes.Clear();
        }

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