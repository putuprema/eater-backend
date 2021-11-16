namespace Application.Products.Commands.EditProduct
{
    public class EditProductCommandHandler : IRequestHandler<EditProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;

        public EditProductCommandHandler(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<Unit> Handle(EditProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.Category.Id != request.CategoryId)
            {
                var category = await _productCategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
                if (category == null)
                    throw new NotFoundException("Product category not found");

                product.PreviousCategoryId = product.Category.Id;
                product.Category = category.Adapt<SimpleProductCategory>();
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Enabled = request.Enabled;

            await _productRepository.UpsertAsync(product, cancellationToken: cancellationToken);
            return Unit.Value;
        }
    }
}
