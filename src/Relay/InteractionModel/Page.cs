namespace Relay.InteractionModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class Page<T> : IPage<T>
    {
        public static readonly IPage<T> Empty = new Page<T>(Array.Empty<T>(), 0, 0, null);

        private readonly IEnumerable<T> items;
        private readonly IPage page;

        public Page(IEnumerable<T> items, int totalCount, int filterCount, IPage page)
        {
            this.items = items;
            this.page = page;
            this.Count = items.Count();
            this.TotalCount = totalCount;
            this.FilterCount = filterCount;
        }

        public int Count { get; }

        public int TotalCount { get; }

        public int FilterCount { get; }

        public int SkipCount => this.page.SkipCount;

        public int TakeCount => this.page.TakeCount;

        public string Search => this.page.Search;

        public string Filter => this.page.Filter;

        public string Sort => this.page.Sort;

        public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();
    }

    internal class SinglePage<T> : IPage<T>
    {
        private readonly IEnumerable<T> items;

        public SinglePage(IEnumerable<T> items)
        {
            this.items = items;
            this.Count = items.Count();
        }

        public int Count { get; }

        public int TotalCount => this.Count;

        public int FilterCount => this.Count;

        public int SkipCount => 0;

        public int TakeCount => this.Count;

        public string Search => null;

        public string Filter => null;

        public string Sort => null;

        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
    }

    /// <summary>
    /// Provides static methods for working with paged collections of items.
    /// </summary>
    public static class Page
    {
        /// <summary>
        /// The number of the first page.
        /// </summary>
        public const int FirstPageNumber = 1;

        /// <summary>
        /// The available page sizes that can be used to limit the number of items in a page.
        /// </summary>
        public static readonly int[] AvailablePageSizes = { 10, 25, 50, 100, 200 };

        /// <summary>
        /// Returns the number of the current page based on the <paramref name="page"/> properties.
        /// </summary>
        /// <param name="page">The page to calculate the number for.</param>
        /// <returns>The number of the current page.</returns>
        public static int GetPageNumber(this IPage page)
        {
            if (page.TakeCount > 0)
                return page.SkipCount / page.TakeCount + FirstPageNumber;

            return FirstPageNumber;
        }

        /// <summary>
        /// Returns the size of the page based on the <paramref name="range"/> properties.
        /// </summary>
        /// <param name="range">The range to calculate the page size for.</param>
        /// <returns>The size of the page.</returns>
        public static int GetPageSize(this IPage range)
        {
            return NormalizePageSize(range.TakeCount);
        }

        /// <summary>
        /// Normalizes the specified page size to the nearest available page size.
        /// </summary>
        /// <param name="pageSize">The page size to normalize.</param>
        /// <returns>The normalized page size.</returns>
        public static int NormalizePageSize(int? pageSize)
        {
            if (pageSize == null)
                return AvailablePageSizes.Min();

            return AvailablePageSizes.Aggregate((x, y) => Math.Abs(x - pageSize.Value) < Math.Abs(y - pageSize.Value) ? x : y);
        }

        /// <summary>
        /// Returns a page of items from the specified <paramref name="source"/> sequence
        /// based on the specified <paramref name="page"/> properties and filter function.
        /// </summary>
        /// <typeparam name="TSource">The type of items in the source sequence.</typeparam>
        /// <param name="source">The source sequence to page.</param>
        /// <param name="filter">The filter function to apply to the source sequence.</param>
        /// <param name="page">The page properties to apply to the source sequence.</param>
        /// <returns>The paged and filtered sequence of items.</returns>
        public static IPage<TSource> ToPage<TSource>(this IEnumerable<TSource> source, Func<TSource, string, bool> filter, IPage page)
        {
            var filteredSource = source;
            var totalCount = -1;
            var filterCount = -1;
            if (page != null)
            {
                // 1. Count items
                filterCount = totalCount = source.Count();
                if (!String.IsNullOrEmpty(page.Filter))
                {
                    filteredSource = source.Where(s => filter(s, page.Filter) && filter(s, page.Search));
                    // 2. Count filtered items
                    filterCount = filteredSource.Count();
                }

                // 3. Sample items
                source = source.Skip(page.SkipCount).Take(page.TakeCount);
            }
            // 4. Select items
            var sample = source.ToArray();

            return From(sample, totalCount, filterCount, page);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Page{T}"/> class from
        /// the specified <paramref name="items"/>, total count, filter count, and page properties.
        /// </summary>
        /// <param name="items">The items to include in the page.</param>
        /// <param name="totalCount">The total number of items in the source sequence.</param>
        /// <param name="filterCount">The number of items in the filtered source sequence.</param>
        /// <param name="page">The page properties that were applied to the items.</param>
        /// <returns>A new instance of the <see cref="IPage{T}"/> containing the specified items.</returns>
        public static IPage<T> From<T>(IEnumerable<T> items, int totalCount, int filterCount, IPage page)
        {
            if (page != null)
                return new Page<T>(items, totalCount, filterCount, page);

            return new SinglePage<T>(items);
        }

        /// <summary>
        /// Returns an empty page of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of items in the page.</typeparam>
        /// <returns>An empty page of the specified type.</returns>
        public static IPage<T> Empty<T>() => Page<T>.Empty;
    }
}
