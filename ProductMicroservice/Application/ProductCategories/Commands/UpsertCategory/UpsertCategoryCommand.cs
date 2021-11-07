namespace Application.ProductCategories.Commands.UpsertCategory
{
    public class UpsertCategoryCommand : IRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class UpsertCategoryCommandValidator : AbstractValidator<UpsertCategoryCommand>
    {
        public UpsertCategoryCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
