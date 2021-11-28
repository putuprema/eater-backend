namespace Eater.Shared.Constants
{
    public static class Events
    {
        public const string PayloadVersion = "1.0";

        public static class InitPayment
        {
            public const string InitPaymentSuccess = "InitPaymentSuccess";
            public const string InitPaymentFailed = "InitPaymentFailed";
        }

        public static class OrderItemValidation
        {
            public const string OrderItemValidationSuccess = "OrderItemValidationSuccess";
            public const string OrderItemValidationFailed = "OrderItemValidationFailed";
        }

        public static class PaymentStatus
        {
            public const string PaymentStatusChanged = "PaymentStatusChanged";
        }

        public static class OrderStatus
        {
            public const string OrderStatusChanged = "OrderStatusChanged";
        }
    }
}
