namespace Application.ProductCategories.Commands.UpsertCategory
{
    public class UpsertCategoryCommandHandler : IRequestHandler<UpsertCategoryCommand>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public UpsertCategoryCommandHandler(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<Unit> Handle(UpsertCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = request.Adapt<ProductCategory>();
            await _productCategoryRepository.UpsertAsync(category, cancellationToken);
            return Unit.Value;
        }
    }
}
