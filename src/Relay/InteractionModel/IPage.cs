namespace Relay.InteractionModel
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a request to filter, sort, and limit a collection of items.
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Gets the number of items to skip from the beginning of the collection.
        /// </summary>
        int SkipCount { get; }

        /// <summary>
        /// Gets the maximum number of items to return in the page.
        /// </summary>
        int TakeCount { get; }

        /// <summary>
        /// Gets the search query to use when filtering the collection.
        /// </summary>
        string Search { get; }

        /// <summary>
        /// Gets the filter query to use when filtering the collection.
        /// </summary>
        string Filter { get; }

        /// <summary>
        /// Gets the sort query to use when sorting the collection.
        /// </summary>
        string Sort { get; }
    }

    /// <summary>
    /// Represents a filtered, limited, and sorted number of items from the data collection
    /// with the offset <see cref="IPage.SkipCount"/> and desired size of <see cref="IPage.TakeCount"/>,
    /// and the total size of <see cref="TotalCount"/>.
    /// </summary>
    public interface IPage<out T> : IPage, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Gets the total number of items in the data collection.
        /// </summary>
        int TotalCount { get; }

        /// <summary>
        /// Gets the number of items in the filtered collection.
        /// </summary>
        int FilterCount { get; }
    }
}
