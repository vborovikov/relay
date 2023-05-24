namespace Relay.PresentationModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Abstracts the View and serves in data binding between the View and the Model.
    /// </summary>
    public abstract class Presenter : PresenterBase, ICommandManager
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
                this.presenter.Status = this.presenter.initialStatus;
            }
        }

        private const string DefaultStatus = "Ready";

        private static ICommandManager globalCommandManager;
        private EventHandler requerySuggested;
        private readonly Dictionary<Delegate, ICommand> commands;
        private int busyCounter;
        private readonly string initialStatus;
        private string status;

        /// <summary>
        /// Initializes a new instance of the <see cref="Presenter"/> class.
        /// </summary>
        protected Presenter() : this(DefaultStatus) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Presenter"/> class.
        /// </summary>
        /// <param name="initialStatus">The initial status message for the presenter.</param>
        protected Presenter(string initialStatus)
        {
            this.commands = new Dictionary<Delegate, ICommand>();
            this.initialStatus = this.status = initialStatus ?? throw new ArgumentNullException(nameof(initialStatus));
        }

        /// <inheritdoc/>
        event EventHandler ICommandManager.RequerySuggested
        {
            add => this.requerySuggested += value;
            remove => this.requerySuggested -= value;
        }

        /// <summary>
        /// Registers a global command manager.
        /// </summary>
        /// <param name="commandManager">The global command manager.</param>
        public static void RegisterCommandManager(ICommandManager commandManager)
        {
            if (globalCommandManager != null)
                throw new InvalidOperationException();

            globalCommandManager = commandManager;
        }

        internal ICommandManager CommandManager => globalCommandManager ?? this;

        /// <summary>
        /// Gets a value indicating whether the presenter is currently busy.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.busyCounter != 0;
            }
        }

        /// <summary>
        /// Gets or sets the status message for the presenter.
        /// </summary>
        public string Status
        {
            get => this.status;
            private set => Set(ref this.status, value);
        }

        /// <summary>
        /// Creates a new <see cref="BusyMonitor"/> instance to track whether the presenter is busy.
        /// </summary>
        /// <returns>A new <see cref="BusyMonitor"/> instance.</returns>
        protected internal IDisposable Busy() => new BusyMonitor(this);

        /// <summary>
        /// Creates a new <see cref="StatusUpdater"/> instance to update the status message for the presenter.
        /// </summary>
        /// <param name="status">The new status message.</param>
        /// <returns>A new <see cref="StatusUpdater"/> instance.</returns>
        protected IDisposable WithStatus(string status = null) => new StatusUpdater(this, status);

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

        /// <summary>
        /// Creates a new <see cref="PresenterCommand"/> instance for the specified <see cref="Func{Task}"/> delegate.
        /// </summary>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whenever the command can be executed.</param>
        /// <returns>Returns the command object.</returns>
        protected virtual ICommand CreateCommand(Func<Task> execute, Func<bool> canExecute)
        {
            return new PresenterCommand(this, execute, canExecute);
        }

        /// <summary>
        /// Creates a new <see cref="PresenterCommand{T}"/> instance for the specified <see cref="Func{T, Task}"/> delegate.
        /// </summary>
        /// <typeparam name="T">The command parameter type.</typeparam>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whenever the command can be executed.</param>
        /// <returns>Returns the command object.</returns>
        protected virtual ICommand CreateCommand<T>(Func<T, Task> execute, Func<T, bool> canExecute)
        {
            return new PresenterCommand<T>(this, execute, canExecute);
        }

        /// <inheritdoc/>
        protected sealed override bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (base.Set(ref storage, value, propertyName))
            {
                this.CommandManager.InvalidateRequerySuggested();
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        void ICommandManager.InvalidateRequerySuggested()
        {
            this.requerySuggested?.Invoke(this, EventArgs.Empty);
        }
    }
}