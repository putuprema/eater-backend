using Application.ProductCategories.Queries.GetCategory;
using Application.Products.Queries.GetProducts;
using System.Collections.Concurrent;

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
            var productsMap = new ConcurrentDictionary<string, IEnumerable<ProductDto>>();

            await Parallel.ForEachAsync(categories, async (category, cancellationToken) =>
            {
                var productQueryResult = await _productRepository.GetByCategoryIdAsync(new GetProductsQuery { CategoryId = category.Id, PageSize = 5 }, cancellationToken);
                productsMap.TryAdd(category.Id, productQueryResult.Items.Adapt<List<ProductDto>>());
            });

            return categories.Select(c => new FeaturedProductsDto
            {
                Category = c.Adapt<ProductCategoryDto>(),
                Products = productsMap[c.Id] ?? new List<ProductDto>()
            });
        }
    }
}
