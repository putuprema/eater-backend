using Application.Payments.Query.GetPaymentInfo;

namespace Application.Payments.Commands.InitPayment
{
    public class InitPaymentCommandHandler : IRequestHandler<InitPaymentCommand, PaymentDto>
    {
        private readonly IPaymentIntegrationService _paymentIntegrationService;
        private readonly IPaymentRepository _paymentRepository;

        public InitPaymentCommandHandler(IPaymentIntegrationService paymentIntegrationService, IPaymentRepository paymentRepository)
        {
            _paymentIntegrationService = paymentIntegrationService;
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDto> Handle(InitPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (payment == null)
            {
                var initResult = await _paymentIntegrationService.InitPaymentAsync(request, cancellationToken);

                payment = await _paymentRepository.UpsertAsync(new Payment
                {
                    OrderId = request.OrderId,
                    CustomerEmail = request.CustomerEmail,
                    Amount = request.Items.Sum(i => i.UnitPrice * i.Quantity),
                    Token = initResult.Token,
                    RedirectUrl = initResult.RedirectUrl
                },
                cancellationToken);
            }

            return payment.Adapt<PaymentDto>();
        }
    }
}
