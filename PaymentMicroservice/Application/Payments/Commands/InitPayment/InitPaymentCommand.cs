using Application.Payments.Query.GetPaymentInfo;

namespace Application.Payments.Commands.InitPayment
{
    public class InitPaymentCommand : IRequest<PaymentDto>
    {
        public string CustomerEmail { get; set; }
        public string OrderId { get; set; }
        public List<PaymentItem> Items { get; set; }
    }

    public class InitPaymentCommandValidator : AbstractValidator<InitPaymentCommand>
    {
        public InitPaymentCommandValidator()
        {
            RuleFor(x => x.CustomerEmail)
                .NotEmpty().WithMessage("Customer email is required")
                .EmailAddress().WithMessage("Customer email is invalid");

            RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required");
            RuleFor(x => x.Items).NotEmpty().WithMessage("Payment items is required");
            RuleForEach(x => x.Items).SetValidator(new PaymentItemValidator());
        }
    }
}
