namespace Application.Products.Commands.BulkUpdateCategory
{
    public class BulkUpdateCategoryCommandHandler : IRequestHandler<BulkUpdateCategoryCommand, BulkUpdateCategoryResult>
    {
        private readonly IProductRepository _productRepository;

        public BulkUpdateCategoryCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<BulkUpdateCategoryResult> Handle(BulkUpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            return await _productRepository.BulkUpdateCategoryDataAsync(request.Category, request.Continuation, cancellationToken);
        }
    }
}
