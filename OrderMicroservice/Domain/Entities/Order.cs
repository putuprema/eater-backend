﻿using Newtonsoft.Json;
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

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderStatus
    {
        VALIDATING,
        PENDING_PAYMENT,
        QUEUED,
        PREPARING,
        READY,
        SERVED,
        CANCELED
    }

    public class Order
    {
        public bool IsNew { get; set; } = true;
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Customer Customer { get; set; }
        public Table Table { get; set; }
        public int Amount { get; set; }
        public List<OrderItem> Items { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.VALIDATING;
        public string CancellationReason { get; set; }
        public Payment Payment { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ServedOn { get; set; }
        [JsonProperty("_etag")]
        public string ETag { get; set; }
    }
}
