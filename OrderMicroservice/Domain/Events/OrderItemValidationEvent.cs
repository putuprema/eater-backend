using Domain.Entities;

namespace Domain.Events
{
    public class OrderItemValidationEvent
    {
        public string ErrorMessage { get; set; }
        public IEnumerable<OrderItem> Products { get; set; }
    }
}
