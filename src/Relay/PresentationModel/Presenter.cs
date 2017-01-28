namespace Relay.PresentationModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Abstracts the View and serves in data binding between the View and the Model.
    /// </summary>
    public abstract class Presenter : INotifyPropertyChanged
    {
        private sealed class BusyMonitor : IDisposable
        {
            private readonly Presenter presenter;
            private bool isDisposed = false;

            public BusyMonitor(Presenter presenter)
            {
                this.presenter = presenter;

                var getBusy = false;
                if (Interlocked.CompareExchange(ref this.presenter.busyCounter, 1, 0) == 0)
                {
                    getBusy = true;
                }
                else
                {
                    Interlocked.Increment(ref this.presenter.busyCounter);
                }

                if (getBusy)
                {
                    this.presenter.RaisePropertyChanged(nameof(Presenter.IsBusy));
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (this.isDisposed == false)
                {
                    if (disposing)
                    {
                        if (Interlocked.Decrement(ref this.presenter.busyCounter) == 0)
                        {
                            this.presenter.RaisePropertyChanged(nameof(Presenter.IsBusy));
                        }
                    }

                    this.isDisposed = true;
                }
            }
        }

        private readonly Dictionary<Delegate, ICommand> commands;
        private int busyCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Presenter"/> class.
        /// </summary>
        protected Presenter()
        {
            this.commands = new Dictionary<Delegate, ICommand>();
        }

        public bool IsBusy
        {
            get
            {
                return this.busyCounter != 0;
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected internal IDisposable Busy()
        {
            return new BusyMonitor(this);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> for the specified <see cref="Func{Task}"/> delegate.
        /// </summary>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whenever the command can be executed.</param>
        /// <returns>Returns the command object.</returns>
        protected ICommand GetCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            ICommand command;
            if (this.commands.TryGetValue(execute, out command))
                return command;

            command = CreateCommand(execute, canExecute);
            this.commands.Add(execute, command);

            return command;
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> for the specified <see cref="Func{T, Task}"/> delegate.
        /// </summary>
        /// <typeparam name="T">The command parameter type.</typeparam>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whenever the command can be executed.</param>
        /// <returns>Returns the command object</returns>
        protected ICommand GetCommand<T>(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            ICommand command = null;
            if (this.commands.TryGetValue(execute, out command))
                return command;

            command = CreateCommand(execute, canExecute);
            this.commands.Add(execute, command);

            return command;
        }

        protected virtual ICommand CreateCommand(Func<Task> execute, Func<bool> canExecute)
        {
            return new PresenterCommand(this, execute, canExecute);
        }

        protected virtual ICommand CreateCommand<T>(Func<T, Task> execute, Func<T, bool> canExecute)
        {
            return new PresenterCommand<T>(this, execute, canExecute);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="args">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName ?? String.Empty));
        }
    }
}