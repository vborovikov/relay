namespace Relay.InteractionModel
{
    using System;

    /// <summary>
    /// Represents a request to filter, sort, and limit a collection of items with a specific page size, search query,
    /// filter query, and sort query.
    /// </summary>
    public class PageRequest : IPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageRequest"/> class with default values.
        /// </summary>
        public PageRequest()
        {
            this.P = Page.FirstPageNumber;
            this.Ps = Page.NormalizePageSize(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRequest"/> class with values from an existing <see cref="IPage"/> instance.
        /// </summary>
        /// <param name="page">The <see cref="IPage"/> instance to initialize the values from.</param>
        public PageRequest(IPage page)
        {
            this.P = page.GetPageNumber();
            this.Ps = page.GetPageSize();
            this.Q = page.Search;
            this.F = page.Filter;
            this.S = page.Sort;
        }

        /// <summary>
        /// Gets or sets the number of the current page.
        /// </summary>
        public int? P { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items to return on a page.
        /// </summary>
        public int? Ps { get; set; }

        /// <summary>
        /// Gets or sets the search query to use when filtering the collection.
        /// </summary>
        public string Q { get; set; }

        /// <summary>
        /// Gets or sets the filter query to use when filtering the collection.
        /// </summary>
        public string F { get; set; }

        /// <summary>
        /// Gets or sets the sort query to use when sorting the collection.
        /// </summary>
        public string S { get; set; }

        int IPage.SkipCount => (Math.Max(Page.FirstPageNumber, this.P ?? Page.FirstPageNumber) - 1) * Page.NormalizePageSize(this.Ps);

        int IPage.TakeCount => Page.NormalizePageSize(this.Ps);

        string IPage.Search => this.Q;

        string IPage.Filter => this.F;

        string IPage.Sort => this.S;
    }
}
