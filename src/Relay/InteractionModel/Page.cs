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

    public static class Page
    {
        public const int FirstPageNumber = 1;

        public static readonly int[] AvailablePageSizes = { 10, 25, 50, 100, 200 };

        public static int GetPageNumber(this IPage page)
        {
            if (page.TakeCount > 0)
                return page.SkipCount / page.TakeCount + FirstPageNumber;

            return FirstPageNumber;
        }

        public static int GetPageSize(this IPage range)
        {
            return NormalizePageSize(range.TakeCount);
        }

        public static int NormalizePageSize(int? pageSize)
        {
            if (pageSize == null)
                return AvailablePageSizes.Min();

            return AvailablePageSizes.Aggregate((x, y) => Math.Abs(x - pageSize.Value) < Math.Abs(y - pageSize.Value) ? x : y);
        }

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

        public static IPage<T> From<T>(IEnumerable<T> items, int totalCount, int filterCount, IPage page)
        {
            if (page != null)
                return new Page<T>(items, totalCount, filterCount, page);

            return new SinglePage<T>(items);
        }

        public static IPage<T> Empty<T>() => Page<T>.Empty;
    }
}
