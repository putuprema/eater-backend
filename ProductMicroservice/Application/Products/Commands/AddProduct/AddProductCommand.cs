using Microsoft.AspNetCore.Http;

namespace Application.Products.Commands.AddProduct
{
    public class AddProductCommand : IRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }
        public string CategoryId { get; set; }
        public IFormFile Image { get; set; }
    }

    public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
    {
        public AddProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Product description is required");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(1000).WithMessage("Product price must be greater than Rp1000");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Product category ID is required");
            RuleFor(x => x.Image).NotNull().WithMessage("Product image is required");
        }
    }
}
