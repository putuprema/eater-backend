namespace Application.Payments
{
    public class PaymentDto
    {
        public string OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public int Amount { get; set; }
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
