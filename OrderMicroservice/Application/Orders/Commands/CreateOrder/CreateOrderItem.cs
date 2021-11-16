namespace Application.Orders.Commands.CreateOrder
{
    public class CreateOrderItem
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }

    public class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Product ID is required");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity is required");
        }
    }
}
