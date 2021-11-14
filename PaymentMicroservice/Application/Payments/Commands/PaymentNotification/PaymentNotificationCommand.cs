using Application.Payments.Query.GetPaymentInfo;

namespace Application.Payments.Commands.PaymentNotification
{
    public class PaymentNotificationCommand : IRequest<PaymentDto>
    {
        public string Payload { get; set; }
        public string Signature { get; set; }
    }
}
