namespace Application.Orders.Commands.CreateOrder
{
    public class CustomerDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class CustomerDetailsValidator : AbstractValidator<CustomerDetails>
    {
        public CustomerDetailsValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Customer ID is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Customer name is required");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Customer email is required")
                .EmailAddress().WithMessage("Customer email is invalid");
        }
    }
}
