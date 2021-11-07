using Application.Products.Queries.GetProducts;

namespace Application.Products.Queries.GetProduct
{
    public class GetProductQuery : IRequest<ProductDto>
    {
        public string Id { get; set; }
    }
}
