namespace Relay.PresentationModel
{
    using System;

    public interface ICommandManager
    {
        event EventHandler RequerySuggested;

        void InvalidateRequerySuggested();
    }
}
