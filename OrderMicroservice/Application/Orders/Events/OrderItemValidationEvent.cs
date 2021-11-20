namespace Application.Orders.Events
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class OrderItemValidationEvent
    {
        public string OrderId { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
