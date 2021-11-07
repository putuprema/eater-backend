namespace Application.Products.Commands.EditProduct
{
    public class EditProductCommand : IRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }
        public bool Enabled { get; set; } = true;
        public string CategoryId { get; set; }
    }

    public class EditProductCommandValidator : AbstractValidator<EditProductCommand>
    {
        public EditProductCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Product description is required");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(1000).WithMessage("Product price must be greater than Rp1000");
            RuleFor(x => x.Enabled).NotNull().WithMessage("Product enabled flag is required");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Product category ID is required");
        }
    }
}
