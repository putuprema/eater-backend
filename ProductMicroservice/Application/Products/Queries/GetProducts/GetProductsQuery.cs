namespace Application.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PagedResultSet<ProductDto>>
    {
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public int PageSize { get; set; } = 30;
        public string ContinuationToken { get; set; }
    }
}
