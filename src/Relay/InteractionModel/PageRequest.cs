namespace Relay.InteractionModel
{
    using System;

    public class PageRequest : IPage
    {
        public PageRequest()
        {
            this.P = Page.FirstPageNumber;
            this.Ps = Page.NormalizePageSize(null);
        }

        public PageRequest(IPage page)
        {
            this.P = page.GetPageNumber();
            this.Ps = page.GetPageSize();
            this.Q = page.Search;
            this.F = page.Filter;
            this.S = page.Sort;
        }

        public int? P { get; set; }

        public int? Ps { get; set; }

        public string Q { get; set; }

        public string F { get; set; }

        public string S { get; set; }

        int IPage.SkipCount => (Math.Max(Page.FirstPageNumber, this.P ?? Page.FirstPageNumber) - 1) * Page.NormalizePageSize(this.Ps);

        int IPage.TakeCount => Page.NormalizePageSize(this.Ps);

        string IPage.Search => this.Q;

        string IPage.Filter => this.F;

        string IPage.Sort => this.S;
    }
}
