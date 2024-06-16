namespace Relay.PresentationModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Abstracts the View and serves in data binding between the View and the Model.
    /// </summary>
    public abstract class PresenterBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterBase"/> class.
        /// </summary>
        protected PresenterBase() { }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Always <c>true</c>.</returns>
        protected bool RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName ?? String.Empty));
            return true;
        }

        /// <summary>
        /// Sets the value of a field and raises the <see cref="PropertyChanged"/> event if the value changed.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="storage">A reference to the field to set.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="propertyName">The name of the property that changed. If not specified, the caller's name is used.</param>
        /// <returns><c>true</c> if the value was changed; otherwise, <c>false</c>.</returns>
        protected virtual bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

            return true;
        }
    }
}