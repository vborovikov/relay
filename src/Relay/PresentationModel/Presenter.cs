namespace Relay.PresentationModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Abstracts the View and serves in data binding between the View and the Model.
    /// </summary>
    public abstract class Presenter : INotifyPropertyChanged, ICommandManager
    {
        private abstract class PresenterHelper : IDisposable
        {
            protected readonly Presenter presenter;
            private bool isDisposed;

            protected PresenterHelper(Presenter presenter)
            {
                this.presenter = presenter;
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected abstract void OnDispose();

            private void Dispose(bool disposing)
            {
                if (!this.isDisposed)
                {
                    if (disposing)
                    {
                        OnDispose();
                    }

                    this.isDisposed = true;
                }
            }
        }

        private sealed class BusyMonitor : PresenterHelper
        {
            public BusyMonitor(Presenter presenter) : base(presenter)
            {
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

            protected override void OnDispose()
            {
                if (Interlocked.Decrement(ref this.presenter.busyCounter) == 0)
                {
                    this.presenter.RaisePropertyChanged(nameof(Presenter.IsBusy));
                }
            }
        }

        private sealed class StatusUpdater : PresenterHelper
        {
            public StatusUpdater(Presenter presenter, string status) : base(presenter)
            {
                this.presenter.Status = status;
            }

            protected override void OnDispose()
            {
                this.presenter.Status = DefaultStatus;
            }
        }

        private const string DefaultStatus = "Ready";

        private static ICommandManager globalCommandManager;
        private EventHandler requerySuggested;
        private readonly Dictionary<Delegate, ICommand> commands;
        private int busyCounter;
        private string status;

        /// <summary>
        /// Initializes a new instance of the <see cref="Presenter"/> class.
        /// </summary>
        protected Presenter()
        {
            this.commands = new Dictionary<Delegate, ICommand>();
            this.status = DefaultStatus;
        }

        event EventHandler ICommandManager.RequerySuggested
        {
            add => this.requerySuggested += value;
            remove => this.requerySuggested -= value;
        }

        public static void RegisterCommandManager(ICommandManager commandManager)
        {
            if (globalCommandManager != null)
                throw new InvalidOperationException();

            globalCommandManager = commandManager;
        }

        internal ICommandManager CommandManager => globalCommandManager ?? this;

        public bool IsBusy
        {
            get
            {
                return this.busyCounter != 0;
            }
        }

        public string Status
        {
            get => this.status;
            private set => Set(ref this.status, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected internal IDisposable Busy() => new BusyMonitor(this);

        protected IDisposable WithStatus(string status = null) => new StatusUpdater(this, status);

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;

            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            this.CommandManager.InvalidateRequerySuggested();

            return true;
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
        /// <returns>Always <c>true</c>.</returns>
        protected bool RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName ?? String.Empty));
            return true;
        }

        void ICommandManager.InvalidateRequerySuggested()
        {
            this.requerySuggested?.Invoke(this, EventArgs.Empty);
        }
    }
}