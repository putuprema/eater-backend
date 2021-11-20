using Application.ProductCategories.Queries.GetCategory;
using Application.Products.Queries.GetFeaturedProducts;
using Application.Products.Queries.GetProducts;
using Mapster;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class FeaturedProductsService : IFeaturedProductsService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private IEnumerable<FeaturedProductsDto> _featuredProducts;

        public FeaturedProductsService(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        private async Task<IEnumerable<FeaturedProductsDto>> GetFeaturedProductsFromDbAsync(CancellationToken cancellationToken = default)
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

        public async Task<IEnumerable<FeaturedProductsDto>> GetFeaturedProducts(CancellationToken cancellationToken = default)
        {
            if (_featuredProducts == null)
            {
                _featuredProducts = await GetFeaturedProductsFromDbAsync(cancellationToken);
            }
            return _featuredProducts;
        }

        public async Task PopulateFeaturedProductsCache(CancellationToken cancellationToken = default)
        {
            _featuredProducts = await GetFeaturedProductsFromDbAsync(cancellationToken);
        }
    }
}
