namespace Relay.ProcessModel;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the possible statuses of a process.
/// </summary>
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
    /// <summary>
    /// Represents a process state.
    /// </summary>
    public sealed class State : ICloneable, IEquatable<State>
    {
        /// <summary>
        /// Gets the unique identifier of the process.
        /// </summary>
        public Guid ProcessId { get; private set; }

        /// <summary>
        /// Gets or sets the status of the process.
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }

        /// <summary>
        /// Gets or sets the count of activities that have been completed.
        /// </summary>
        public int CompletedActivityCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the activity being compensated.
        /// </summary>
        public int CompensationIndex { get; set; }

        /// <summary>
        /// Gets or sets the exception that occurred during process execution.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        private State()
        {
            this.ProcessId = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="State"/> class.
        /// </summary>
        public static State Create()
        {
            return new State();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="State"/> class that is a copy of the current instance.
        /// </summary>
        public State Clone() => (State)MemberwiseClone();

        object ICloneable.Clone() => Clone();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as State);
        }

        /// <inheritdoc />
        public bool Equals(State other)
        {
            return other != null &&
                   this.ProcessId.Equals(other.ProcessId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return -506264241 + this.ProcessId.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="State"/> objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><c>true</c> if a and b are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(State left, State right)
        {
            return EqualityComparer<State>.Default.Equals(left, right);
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="State"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns><c>true</c> if a and b are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(State left, State right)
        {
            return !(left == right);
        }
    }
}