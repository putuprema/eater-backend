namespace Domain.Entities
{
    public class Payment
    {
        public PaymentStatus Status { get; set; }
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
