using Application.Products.GetProducts;

namespace API.Functions.DurableFunctions.OrderCreated
{
    public class OrderItemValidationEventData
    {
        public string OrderId { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
