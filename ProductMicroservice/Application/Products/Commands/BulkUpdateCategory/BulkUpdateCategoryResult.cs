namespace Application.Products.Commands.BulkUpdateCategory
{
    public class BulkUpdateCategoryResult
    {
        public int UpdatedItems { get; set; }
        public BulkUpdateCategoryContinuation Continuation { get; set; }
    }

    public class BulkUpdateCategoryContinuation
    {
        public int LastCount { get; set; }
        public string Token { get; set; }
    }
}
