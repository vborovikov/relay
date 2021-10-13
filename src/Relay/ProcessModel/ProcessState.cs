namespace Relay.ProcessModel
{
    using System;
    using System.Collections.Generic;

    public enum ProcessStatus
    {
        /// <summary>
        /// This process has not yet been registered and begun executing.
        /// </summary>
        NotStarted,
        /// <summary>
        /// This process is currently executing (moving forward).
        /// </summary>
        Executing,
        /// <summary>
        /// This process has completed successfully.
        /// </summary>
        Executed,
        /// <summary>
        /// This process is currently compensating after a execution failure (moving backward).
        /// </summary>
        Compensating,
        /// <summary>
        /// This process has completed compensating after a failure.
        /// </summary>
        Compensated,
        /// <summary>
        /// This process has completed compensating after an abort.
        /// </summary>
        Aborted
    }

    public partial class Process
    {
        public class State : ICloneable, IEquatable<State>
        {
            public Guid ProcessId { get; private set; }

            public ProcessStatus ProcessStatus { get; set; }

            public int CompletedActivityCount { get; set; }

            public int CompensationIndex { get; set; }

            public Exception Exception { get; set; }

            private State()
            {
                this.ProcessId = Guid.NewGuid();
            }

            public static State Create()
            {
                return new State();
            }

            public State Clone() => (State)MemberwiseClone();

            object ICloneable.Clone() => Clone();

            public override bool Equals(object obj)
            {
                return Equals(obj as State);
            }

            public bool Equals(State other)
            {
                return other != null &&
                       this.ProcessId.Equals(other.ProcessId);
            }

            public override int GetHashCode()
            {
                return -506264241 + this.ProcessId.GetHashCode();
            }

            public static bool operator ==(State left, State right)
            {
                return EqualityComparer<State>.Default.Equals(left, right);
            }

            public static bool operator !=(State left, State right)
            {
                return !(left == right);
            }
        }
    }
}