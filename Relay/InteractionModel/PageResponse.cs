namespace Relay.InteractionModel
{
    using System.Linq;

    /// <summary>
    /// Represents a response that contains a page of items of type <typeparamref name="T"/>, along with metadata about the page.
    /// </summary>
    /// <typeparam name="T">The type of items in the page.</typeparam>
    public class PageResponse<T> : IPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageResponse{T}"/> class with default values.
        /// </summary>
        public PageResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResponse{T}"/> class with the specified items and page metadata.
        /// </summary>
        /// <param name="items">The items in the page.</param>
        /// <param name="page">The metadata for the page.</param>
        public PageResponse(T[] items, IPage page)
        {
            this.Items = items;
            this.Count = this.Items.Length;
            this.SkipCount = page.SkipCount;
            this.TakeCount = page.TakeCount;
            this.Search = page.Search;
            this.Filter = page.Filter;
            this.Sort = page.Sort;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResponse{T}"/> class with the specified items and page request metadata.
        /// </summary>
        /// <param name="items">The items in the page.</param>
        /// <param name="page">The request metadata for the page.</param>
        public PageResponse(T[] items, PageRequest page) : this(items, (IPage)page)
        {
            this.PageNumber = page.P;
            this.PageSize = page.Ps;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResponse{T}"/> class with the specified page metadata.
        /// </summary>
        /// <param name="page">The metadata for the page.</param>
        public PageResponse(IPage<T> page) : this(page.ToArray(), page)
        {
            this.TotalCount = page.TotalCount;
            this.FilterCount = page.FilterCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageResponse{T}"/> class with the specified items and page request metadata.
        /// </summary>
        /// <param name="page">The request metadata for the page.</param>
        /// <param name="request">The request metadata for the page.</param>
        public PageResponse(IPage<T> page, PageRequest request) : this(page.ToArray(), request)
        {
            this.TotalCount = page.TotalCount;
            this.FilterCount = page.FilterCount;
        }

        /// <summary>
        /// Gets the items in the page.
        /// </summary>
        public T[] Items { get; }

        /// <summary>
        /// Gets the total number of items in the source sequence.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the number of items in the filtered source sequence.
        /// </summary>
        public int FilterCount { get; }

        /// <summary>
        /// Gets the number of items to skip from the beginning of the collection.
        /// </summary>
        public int SkipCount { get; }

        /// <summary>
        /// Gets the maximum number of items to return in the page.
        /// </summary>
        public int TakeCount { get; }

        /// <summary>
        /// Gets or sets the number of items in the page.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the number of the current page.
        /// </summary>
        public int? PageNumber { get; }

        /// <summary>
        /// Gets the maximum number of items to return on a page.
        /// </summary>
        public int? PageSize { get; }

        /// <summary>
        /// Gets the search query to use when filtering the collection.
        /// </summary>
        public string Search { get; }

        /// <summary>
        /// Gets the filter query to use when filtering the collection.
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// Gets the sort query to use when sorting the collection.
        /// </summary>
        public string Sort { get; }

        /// <summary>
        /// Returns a new instance of the <see cref="Page{T}"/> class 
        /// containing the items and metadata from this <see cref="PageResponse{T}"/> instance.
        /// </summary>
        /// <returns>A new instance of the <see cref="Page{T}"/> class
        /// containing the items and metadata from this <see cref="PageResponse{T}"/> instance.</returns>
        public IPage<T> ToPage() => 
            Page.From(this.Items, this.TotalCount, this.FilterCount, this);
    }
}
