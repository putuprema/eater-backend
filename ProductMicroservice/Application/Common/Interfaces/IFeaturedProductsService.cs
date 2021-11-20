using Application.Products.Queries.GetFeaturedProducts;

namespace Application.Common.Interfaces
{
    public interface IFeaturedProductsService
    {
        Task<IEnumerable<FeaturedProductsDto>> GetFeaturedProducts(CancellationToken cancellationToken = default);
        Task PopulateFeaturedProductsCache(CancellationToken cancellationToken = default);
    }
}
