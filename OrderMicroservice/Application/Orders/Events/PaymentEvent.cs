namespace Application.Orders.Events
{
    public class PaymentEvent
    {
        public string OrderId { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
