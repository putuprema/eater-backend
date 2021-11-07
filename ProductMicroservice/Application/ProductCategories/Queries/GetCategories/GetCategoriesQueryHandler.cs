using Application.ProductCategories.Queries.GetCategory;

namespace Application.ProductCategories.Queries.GetCategories
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<ProductCategoryDto>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public GetCategoriesQueryHandler(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<IEnumerable<ProductCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var list = await _productCategoryRepository.GetAllAsync(cancellationToken);
            return list.Adapt<List<ProductCategoryDto>>();
        }
    }
}
