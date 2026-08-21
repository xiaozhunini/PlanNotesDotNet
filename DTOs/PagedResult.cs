namespace PlanNoteServer.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public int TotalCount { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasNextPage => PageIndex < TotalPages;

        public bool HasPreviousPage => PageIndex > 1;
    }
}