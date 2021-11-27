namespace Domain.Events
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Notes { get; set; }
    }

    public class OrderCreatedEvent
    {
        public string Id { get; set; }
        public List<OrderItem> Items { get; set; }
    }
}
