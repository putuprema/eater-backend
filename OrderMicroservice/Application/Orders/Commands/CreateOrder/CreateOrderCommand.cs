using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public CustomerDetails Customer { get; set; }
        public TableDetails Table { get; set; }
        public List<CreateOrderItem> Items { get; set; }
    }

    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.Customer)
                .NotNull().WithMessage("Customer data is required")
                .SetValidator(new CustomerDetailsValidator());

            RuleFor(x => x.Table)
                .NotNull().WithMessage("Table data is required")
                .SetValidator(new TableDetailsValidator());

            RuleFor(x => x.Items).NotEmpty().WithMessage("Items is required");
            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        }
    }
}
