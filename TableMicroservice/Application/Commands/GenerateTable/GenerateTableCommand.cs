namespace Application.Commands.GenerateTable
{
    public class GenerateTableCommand : IRequest
    {
        public int Quantity { get; set; }
    }

    public class GenerateTableCommandValidator : AbstractValidator<GenerateTableCommand>
    {
        public GenerateTableCommandValidator()
        {
            RuleFor(x => x.Quantity)
                .InclusiveBetween(1, 20)
                .WithMessage("Only 1 to 20 table requests are allowed at a time");
        }
    }
}
