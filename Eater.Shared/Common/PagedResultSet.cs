namespace Eater.Shared.Common
{
    public class PagedResultSet<T>
    {
        public string? ContinuationToken { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
