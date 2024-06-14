namespace Relay.InteractionModel
{
    using System;
    using System.Text;

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageRequest"/> class with values from an existing <see cref="IPage"/> instance.
        /// </summary>
        /// <param name="page">The <see cref="IPage"/> instance to initialize the values from.</param>
        public PageRequest(IPage page)
        {
            this.P = page.GetPageNumber();
            this.Ps = NormalizePageSizeOverride(page.TakeCount);
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

        int IPage.SkipCount => (Math.Max(Page.FirstPageNumber, this.P ?? Page.FirstPageNumber) - 1) * NormalizePageSizeOverride(this.Ps);

        int IPage.TakeCount => NormalizePageSizeOverride(this.Ps);

        string IPage.Search => this.Q;

        string IPage.Filter => this.F;

        string IPage.Sort => this.S;

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"?p={this.GetPageNumber()}&ps={NormalizePageSizeOverride(this.Ps)}");
            
            if (!string.IsNullOrWhiteSpace(this.Q))
            {
                sb.Append($"&q={Uri.EscapeDataString(this.Q)}");
            }
            if (!string.IsNullOrWhiteSpace(this.F))
            {
                sb.Append($"&f={Uri.EscapeDataString(this.F)}");
            }
            if (!string.IsNullOrWhiteSpace(this.S))
            {
                sb.Append($"&s={Uri.EscapeDataString(this.S)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Normalizes the specified page size to the nearest available page size.
        /// </summary>
        /// <param name="pageSize">The page size to normalize.</param>
        /// <returns>The normalized page size.</returns>
        protected virtual int NormalizePageSizeOverride(int? pageSize)
        {
            return Page.NormalizePageSize(pageSize);
        }
    }
}
