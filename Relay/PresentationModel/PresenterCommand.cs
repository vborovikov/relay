namespace Relay.PresentationModel
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Represents the <see cref="Presenter"/> command.
    /// </summary>
    public abstract class PresenterCommandBase : ICommand
    {
        private readonly Presenter presenter;
        private bool isExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterCommandBase"/> class.
        /// </summary>
        /// <param name="presenter">The owner of the command.</param>
        protected PresenterCommandBase(Presenter presenter)
        {
            this.presenter = presenter;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="PresenterCommandBase"/> class from being created.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        private PresenterCommandBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        {
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { this.presenter.CommandManager.RequerySuggested += value; }
            remove { this.presenter.CommandManager.RequerySuggested -= value; }
        }


        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public bool CanExecute(object? parameter)
        {
            return !this.isExecuting && CanExecuteOverride(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        public async void Execute(object? parameter)
        {
            try
            {
                BeginExecuting();

                using (this.presenter.Busy())
                {
                    await ExecuteOverrideAsync(parameter);
                }
            }
            finally
            {
                EndExecuting();
            }
        }

        /// <summary>
        /// Called when the command state changes.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            this.presenter.CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// When overridden in a derived class, defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        protected abstract bool CanExecuteOverride(object? parameter);

        /// <summary>
        /// When overridden in a derived class, defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        protected abstract Task ExecuteOverrideAsync(object? parameter);

        private void EndExecuting()
        {
            this.isExecuting = false;
            OnCanExecuteChanged(EventArgs.Empty);
        }

        private void BeginExecuting()
        {
            this.isExecuting = true;
            OnCanExecuteChanged(EventArgs.Empty);
        }
    }

    /// <summary>
    /// Represents the <see cref="Presenter"/> command which requires a parameter.
    /// </summary>
    public class PresenterCommand : PresenterCommandBase
    {
        private readonly Func<bool>? canExecute;
        private readonly Func<Task> execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterCommand"/> class.
        /// </summary>
        /// <param name="presenter">The <see cref="Presenter"/> that created this command.</param>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whether the command can be executed.</param>
        public PresenterCommand(Presenter presenter, Func<Task> execute, Func<bool>? canExecute)
            : base(presenter)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterCommand"/> class.
        /// </summary>
        /// <param name="presenter">The <see cref="Presenter"/> that created this command.</param>
        /// <param name="execute">The execution entry point for the command.</param>
        public PresenterCommand(Presenter presenter, Func<Task> execute)
            : this(presenter, execute, null)
        {
        }

        /// <summary>
        /// When overridden in a derived class, defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        protected override bool CanExecuteOverride(object? parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        /// <summary>
        /// When overridden in a derived class, defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        protected override Task ExecuteOverrideAsync(object? parameter)
        {
            return this.execute();
        }
    }

    /// <summary>
    /// Represents the <see cref="Presenter"/> command which requires a parameter.
    /// </summary>
    /// <typeparam name="T">The command parameter type.</typeparam>
    public class PresenterCommand<T> : PresenterCommandBase
    {
        private readonly Func<T, bool>? canExecute;
        private readonly Func<T, Task> execute;

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterCommand{T}"/> class.
        /// </summary>
        /// <param name="presenter">The <see cref="Presenter"/> that created this command.</param>
        /// <param name="execute">The execution entry point for the command.</param>
        /// <param name="canExecute">The delegate that determines whether the command can be executed.</param>
        public PresenterCommand(Presenter presenter, Func<T, Task> execute, Func<T, bool>? canExecute)
            : base(presenter)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PresenterCommand{T}"/> class.
        /// </summary>
        /// <param name="presenter">The <see cref="Presenter"/> that created this command.</param>
        /// <param name="execute">The execution entry point for the command.</param>
        public PresenterCommand(Presenter presenter, Func<T, Task> execute) : this(presenter, execute, null) { }

        /// <summary>
        /// When overridden in a derived class, defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        protected override bool CanExecuteOverride(object? parameter)
        {
            return (parameter != null && parameter is T typedParam) && (this.canExecute == null || this.canExecute(typedParam));
        }

        /// <summary>
        /// When overridden in a derived class, defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
        protected override Task ExecuteOverrideAsync(object? parameter)
        {
            return this.execute((T)parameter!);
        }
    }
}