namespace Application.Payments.Commands.PaymentNotification
{
    public class PaymentNotificationEvent
    {
        public string OrderId { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
