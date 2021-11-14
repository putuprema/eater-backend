using Application.Payments.Query.GetPaymentInfo;

namespace Application.Payments.Commands.PaymentNotification
{
    public class PaymentNotificationCommandHandler : IRequestHandler<PaymentNotificationCommand, PaymentDto>
    {
        private readonly IPaymentIntegrationService _paymentIntegrationService;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentNotificationCommandHandler(IPaymentIntegrationService paymentIntegrationService, IPaymentRepository paymentRepository)
        {
            _paymentIntegrationService = paymentIntegrationService;
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDto> Handle(PaymentNotificationCommand request, CancellationToken cancellationToken)
        {
            var paymentNotificationEvent = _paymentIntegrationService.HandlePaymentNotification(request.Payload, request.Signature);
            if (paymentNotificationEvent == null)
                return null;

            var payment = await _paymentRepository.GetByOrderIdAsync(paymentNotificationEvent.OrderId, cancellationToken);
            if (payment == null)
                return null;

            if (payment.Status != PaymentStatus.PAID)
            {
                payment.Status = paymentNotificationEvent.Status;
                payment = await _paymentRepository.UpsertAsync(payment, cancellationToken);
            }

            return payment.Adapt<PaymentDto>();
        }
    }
}
