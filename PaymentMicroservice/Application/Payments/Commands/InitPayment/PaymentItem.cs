namespace Application.Payments.Commands.InitPayment
{
    public class PaymentItem
    {
        public string Name { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }

    public class PaymentItemValidator : AbstractValidator<PaymentItem>
    {
        public PaymentItemValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(10000).WithMessage("Product unit price must be a minimum of Rp10000");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Product quantity is required");
            RuleFor(x => x.Image).NotEmpty().WithMessage("Product image url is required");
        }
    }
}
