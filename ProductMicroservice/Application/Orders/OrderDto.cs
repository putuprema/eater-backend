namespace Application.Orders
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Notes { get; set; }
    }

    public class OrderDto
    {
        public string Id { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
