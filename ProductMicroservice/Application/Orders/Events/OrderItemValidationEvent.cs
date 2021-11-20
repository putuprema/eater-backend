using Application.Products.Queries.GetProducts;

namespace Application.Orders.Events
{
    public class OrderItemValidationEvent
    {
        public string OrderId { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
