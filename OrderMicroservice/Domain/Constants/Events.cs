namespace Domain.Constants
{
    public static class Events
    {
        public const string EventDataVersion = "1.0";
        public const string OrderCreated = "OrderCreated";
        public const string OrderItemValidationEvent = "OrderItemValidationEvent";
        public const string OrderStatusChanged = "OrderStatusChanged";
        public const string InitPaymentEvent = "InitPaymentEvent";
        public const string PaymentStatusChanged = "PaymentStatusChanged";
    }
}
