namespace Relay.InteractionModel
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a request to filter, sort, and limit a collection of items.
    /// </summary>
    public interface IPage
    {
        int SkipCount { get; }

        int TakeCount { get; }

        string Search { get; }

        string Filter { get; }

        string Sort { get; }
    }

    /// <summary>
    /// Represents a filtered, limited, and sorted number of items from the data collection
    /// with the offset <see cref="SkipCount"/> and desired size of <see cref="TakeCount"/>,
    /// and the total size of <see cref="TotalCount"/>.
    /// </summary>
    public interface IPage<out T> : IPage, IReadOnlyCollection<T>
    {
        int TotalCount { get; }

        int FilterCount { get; }
    }
}
