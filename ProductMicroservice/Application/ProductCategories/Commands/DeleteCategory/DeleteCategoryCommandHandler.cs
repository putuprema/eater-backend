using Application.Products.Queries.GetProducts;

namespace Application.ProductCategories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;

        public DeleteCategoryCommandHandler(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
        }

        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var existingProducts = await _productRepository.FindAllAsync(new GetProductsQuery { CategoryId = request.Id, PageSize = 1 }, cancellationToken);
            if (existingProducts.Items.Any())
            {
                throw new ForbiddenException("Please delete all associated products first before deleting this category");
            }

            await _productCategoryRepository.DeleteAsync(request.Id, cancellationToken);
            return Unit.Value;
        }
    }
}
