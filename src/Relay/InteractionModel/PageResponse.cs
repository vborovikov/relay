namespace Relay.InteractionModel
{
    using System.Linq;

    public class PageResponse<T> : IPage
    {
        public PageResponse()
        {
        }

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

        public PageResponse(T[] items, PageRequest page) : this(items, (IPage)page)
        {
            this.PageNumber = page.P;
            this.PageSize = page.Ps;
        }

        public PageResponse(IPage<T> page) : this(page.ToArray(), page)
        {
            this.TotalCount = page.TotalCount;
            this.FilterCount = page.FilterCount;
        }

        public PageResponse(IPage<T> page, PageRequest request) : this(page.ToArray(), request)
        {
            this.TotalCount = page.TotalCount;
            this.FilterCount = page.FilterCount;
        }

        public T[] Items { get; set; }

        public int TotalCount { get; set; }

        public int FilterCount { get; set; }

        public int SkipCount { get; set; }

        public int TakeCount { get; set; }

        public int Count { get; set; }

        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public string Search { get; set; }

        public string Filter { get; set; }

        public string Sort { get; set; }

        public IPage<T> ToPage() => 
            Page.From(this.Items, this.TotalCount, this.FilterCount, this);
    }
}
