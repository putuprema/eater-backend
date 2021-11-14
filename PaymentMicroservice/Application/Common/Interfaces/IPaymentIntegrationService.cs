using Application.Payments.Commands.InitPayment;
using Application.Payments.Commands.PaymentNotification;

namespace Application.Common.Interfaces
{
    public interface IPaymentIntegrationService
    {
        PaymentNotificationEvent HandlePaymentNotification(string payloadStr, string signature = default);
        Task<InitPaymentResult> InitPaymentAsync(InitPaymentCommand request, CancellationToken cancellationToken = default);
    }
}
