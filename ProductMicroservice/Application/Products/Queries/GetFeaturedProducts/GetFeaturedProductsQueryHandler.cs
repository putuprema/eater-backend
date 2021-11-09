using Application.ProductCategories.Queries.GetCategory;
using Application.Products.Queries.GetProducts;
using System.Collections.Concurrent;

namespace Application.Products.Queries.GetFeaturedProducts
{
    public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, IEnumerable<FeaturedProductsDto>>
    {
        private readonly IFeaturedProductsService _featuredProductsService;

        public GetFeaturedProductsQueryHandler(IFeaturedProductsService featuredProductsService)
        {
            _featuredProductsService = featuredProductsService;
        }

        public async Task<IEnumerable<FeaturedProductsDto>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _featuredProductsService.GetFeaturedProducts(cancellationToken);
        }
    }
}
