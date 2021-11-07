namespace Domain.Entities
{
    public class PagedResultSet<T>
    {
        public string ContinuationToken { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
