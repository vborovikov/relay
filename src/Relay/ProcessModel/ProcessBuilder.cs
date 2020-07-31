namespace Relay.ProcessModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class Process
    {
        public class Builder
        {
            public abstract class ActivityBuilderBase
            {
                protected readonly Builder processBuilder;

                protected ActivityBuilderBase(Builder processBuilder)
                {
                    this.processBuilder = processBuilder;
                }

                public Process BuildProcess()
                {
                    return this.processBuilder.Buid();
                }
            }

            public class ActivityBuilder<TProcessContext> : ActivityBuilderBase
            {
                private readonly TProcessContext context;

                public ActivityBuilder(Builder processBuilder, TProcessContext context)
                    : base(processBuilder)
                {
                    this.context = context;
                }

                public ActivityBuilder<TProcessContext> AddActivity(
                    Func<TProcessContext, CancellationToken, Task> execute,
                    Func<TProcessContext, CancellationToken, Task> compensate)
                {
                    this.processBuilder.Add(new DelegateActivity<TProcessContext>(context, execute, compensate));
                    return new ActivityBuilder<TProcessContext>(this.processBuilder, context);
                }

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

            private readonly Process.State processState;
            private readonly List<IActivity> activities;

            internal Builder() : this(Process.State.Create())
            {
            }

            internal Builder(Process.State processState)
            {
                this.processState = processState;
                this.activities = new List<IActivity>();
            }

            public void Add(IActivity activity)
            {
                this.activities.Add(activity);
            }

            public Process Buid()
            {
                return new Process(this.processState, this.activities);
            }
        }

        public static Builder.ActivityBuilder<TProcessContext> CreateProcess<TProcessContext>(TProcessContext context)
        {
            return new Builder.ActivityBuilder<TProcessContext>(new Builder(), context);
        }

        public static Builder.ActivityBuilder<TProcessContext> LoadProcess<TProcessContext>(Process.State state, TProcessContext context)
        {
            return new Builder.ActivityBuilder<TProcessContext>(new Builder(state), context);
        }
    }
}
