namespace Application.ProductCategories.Queries.GetCategory
{
    public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, ProductCategoryDto>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public GetCategoryQueryHandler(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<ProductCategoryDto> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var category = await _productCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
            return category.Adapt<ProductCategoryDto>();
        }
    }
}
