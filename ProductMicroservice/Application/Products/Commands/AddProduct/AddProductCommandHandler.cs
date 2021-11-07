namespace Application.Products.Commands.AddProduct
{
    public class AddProductCommandHandler : IRequestHandler<AddProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IStorageService _storageService;

        public AddProductCommandHandler(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository, IStorageService storageService)
        {
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _storageService = storageService;
        }

        public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            var category = await _productCategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                throw new NotFoundException("Product category not found");
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = category.Adapt<SimpleProductCategory>()
            };

            var fileName = product.Id + Path.GetExtension(request.Image.FileName);
            product.ImageUrl = await _storageService.UploadProductPhotoAsync(request.Image, fileName, cancellationToken);

            await _productRepository.UpsertAsync(product, cancellationToken);
            return Unit.Value;
        }
    }
}
