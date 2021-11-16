namespace Application.Orders.Commands.CreateOrder
{
    public class TableDetails
    {
        public string Id { get; set; }
        public int Number { get; set; }
    }

    public class TableDetailsValidator : AbstractValidator<TableDetails>
    {
        public TableDetailsValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Table ID is required");
            RuleFor(x => x.Number).GreaterThan(0).WithMessage("Table number is required");
        }
    }
}
