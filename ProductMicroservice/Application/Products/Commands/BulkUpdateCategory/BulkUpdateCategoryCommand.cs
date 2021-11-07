namespace Application.Products.Commands.BulkUpdateCategory
{
    public class BulkUpdateCategoryCommand : IRequest<BulkUpdateCategoryResult>
    {
        public ProductCategory Category { get; set; }
        public BulkUpdateCategoryContinuation Continuation { get; set; }
    }
}
