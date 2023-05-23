namespace Relay.ProcessModel;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Process
{
    /// <summary>
    /// A builder class for building a process with activities.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// A base class for building activities for a process.
        /// </summary>
        public abstract class ActivityBuilderBase
        {
            /// <summary>
            /// The process builder.
            /// </summary>
            protected readonly Builder processBuilder;

            /// <summary>
            /// Initializes a new instance of the ActivityBuilderBase class.
            /// </summary>
            /// <param name="processBuilder">The process builder.</param>
            protected ActivityBuilderBase(Builder processBuilder)
            {
                this.processBuilder = processBuilder;
            }

            /// <summary>
            /// Builds the process.
            /// </summary>
            /// <returns>The built process.</returns>
            public Process BuildProcess()
            {
                return this.processBuilder.Buid();
            }
        }

        /// <summary>
        /// A builder class for building activities for a process with a context of type TProcessContext.
        /// </summary>
        /// <typeparam name="TProcessContext">The type of the process context.</typeparam>
        public class ActivityBuilder<TProcessContext> : ActivityBuilderBase
        {
            /// <summary>
            /// The process context.
            /// </summary>
            private readonly TProcessContext context;

            /// <summary>
            /// Initializes a new instance of the ActivityBuilder class.
            /// </summary>
            /// <param name="processBuilder">The process builder.</param>
            /// <param name="context">The process context.</param>
            public ActivityBuilder(Builder processBuilder, TProcessContext context)
                : base(processBuilder)
            {
                this.context = context;
            }

            /// <summary>
            /// Adds an activity to the process with an execute and compensate function.
            /// </summary>
            /// <param name="execute">The execute function.</param>
            /// <param name="compensate">The compensate function.</param>
            /// <returns>The activity builder.</returns>
            public ActivityBuilder<TProcessContext> AddActivity(
                Func<TProcessContext, CancellationToken, Task> execute,
                Func<TProcessContext, CancellationToken, Task> compensate)
            {
                this.processBuilder.Add(new DelegateActivity<TProcessContext>(context, execute, compensate));
                return new ActivityBuilder<TProcessContext>(this.processBuilder, context);
            }

            /// <summary>
            /// Adds an activity to the process with an execute and compensate action.
            /// </summary>
            /// <param name="execute">The execute action.</param>
            /// <param name="compensate">The compensate action.</param>
            /// <returns>The activity builder.</returns>
            public ActivityBuilder<TProcessContext> AddActivity(
                Action<TProcessContext, CancellationToken> execute,
                Action<TProcessContext, CancellationToken> compensate)
            {
                this.processBuilder.Add(new DelegateActivity<TProcessContext>(context,
                    (ctx, ct) => Task.Run(() => execute(ctx, ct), ct),
                    (ctx, ct) => Task.Run(() => compensate(ctx, ct), ct)));
                return new ActivityBuilder<TProcessContext>(this.processBuilder, context);
            }
        }

        /// <summary>
        /// The process state.
        /// </summary>
        private readonly Process.State processState;
        /// <summary>
        /// The list of activities.
        /// </summary>
        private readonly List<IActivity> activities;

        /// <summary>
        /// Initializes a new instance of the Builder class.
        /// </summary>
        internal Builder() : this(Process.State.Create())
        {
        }

        /// <summary>
        /// Initializes a new instance of the Builder class with a process state.
        /// </summary>
        /// <param name="processState">The process state.</param>
        internal Builder(Process.State processState)
        {
            this.processState = processState;
            this.activities = new List<IActivity>();
        }

        /// <summary>
        /// Adds an activity to the process.
        /// </summary>
        /// <param name="activity">The activity to add.</param>
        public void Add(IActivity activity)
        {
            this.activities.Add(activity);
        }

        /// <summary>
        /// Builds the process.
        /// </summary>
        /// <returns>The built process.</returns>
        public Process Buid()
        {
            return new Process(this.processState, this.activities);
        }
    }

    /// <summary>
    /// Creates a new process builder with a context of type TProcessContext.
    /// </summary>
    /// <typeparam name="TProcessContext">The type of the process context.</typeparam>
    /// <param name="context">The process context.</param>
    /// <returns>The process builder.</returns>
    public static Builder.ActivityBuilder<TProcessContext> CreateProcess<TProcessContext>(TProcessContext context)
    {
        return new Builder.ActivityBuilder<TProcessContext>(new Builder(), context);
    }

    /// <summary>
    /// Create a new process builder with a context of type TProcessContext and the existing process state.
    /// </summary>
    /// <typeparam name="TProcessContext">The type of the process context.</typeparam>
    /// <param name="state">The process state.</param>
    /// <param name="context">The process context.</param>
    /// <returns>The process builder.</returns>
    public static Builder.ActivityBuilder<TProcessContext> LoadProcess<TProcessContext>(Process.State state, TProcessContext context)
    {
        return new Builder.ActivityBuilder<TProcessContext>(new Builder(state), context);
    }
}
