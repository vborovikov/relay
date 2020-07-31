namespace Relay.ProcessModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IActivity
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
        Task CompensateAsync(CancellationToken cancellationToken);
    }

    abstract class Activity : IActivity
    {
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        public abstract Task CompensateAsync(CancellationToken cancellationToken);
    }

    abstract class Activity<TContext> : Activity
    {
        protected Activity(TContext context)
        {
            this.Context = context;
        }

        public TContext Context { get; }
    }

    class DelegateActivity<TContext> : Activity<TContext>
    {
        private readonly Func<TContext, CancellationToken, Task> execute;
        private readonly Func<TContext, CancellationToken, Task> compensate;

        public DelegateActivity(TContext context,
            Func<TContext, CancellationToken, Task> execute,
            Func<TContext, CancellationToken, Task> compensate)
            : base(context)
        {
            this.execute = execute;
            this.compensate = compensate;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken) =>
            this.execute(this.Context, cancellationToken);

        public override Task CompensateAsync(CancellationToken cancellationToken) =>
            this.compensate(this.Context, cancellationToken);
    }
}