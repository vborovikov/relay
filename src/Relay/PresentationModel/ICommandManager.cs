namespace Relay.PresentationModel
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Defines the commanding Execute/CanExecute events.
    /// </summary>
    public interface ICommandManager
    {
        /// <summary>
        /// Raised when <see cref="ICommand.CanExecute(object)">CanExecute</see> should be requeried on commands.
        /// </summary>
        event EventHandler RequerySuggested;

        /// <summary>
        /// Invokes <see cref="RequerySuggested"/> listeners registered on the current thread.
        /// </summary>
        void InvalidateRequerySuggested();
    }
}
