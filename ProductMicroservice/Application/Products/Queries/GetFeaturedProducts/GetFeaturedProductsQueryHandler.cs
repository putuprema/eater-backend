using Application.ProductCategories.Queries.GetCategory;
using Application.Products.Queries.GetProducts;

namespace Application.Products.Queries.GetFeaturedProducts
{
    public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, IEnumerable<FeaturedProductsDto>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductRepository _productRepository;

        public GetFeaturedProductsQueryHandler(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<FeaturedProductsDto>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
        {
            var categories = await _productCategoryRepository.GetAllAsync(cancellationToken);
            var productsMap = await _productRepository.GetFeaturedProductsPerCategoryAsync(categories.Select(c => c.Id), cancellationToken);

            return categories.Select(c => new FeaturedProductsDto
            {
                Category = c.Adapt<ProductCategoryDto>(),
                Products = productsMap[c.Id] != null ? productsMap[c.Id].Adapt<List<ProductDto>>() : new List<ProductDto>()
            });
        }
    }
}
