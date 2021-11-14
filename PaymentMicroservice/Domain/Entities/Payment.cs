using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.Entities
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentStatus
    {
        UNPAID,
        PAID,
        REJECTED,
        EXPIRED
    }

    public class Payment
    {
        [JsonProperty("id")]
        public string OrderId { get; set; }
        public string CustomerEmail { get; set; }
        public int Amount { get; set; }
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.UNPAID;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    }
}
