using Application.Products.GetProducts;
using Domain.Entities;

namespace API.Functions.DurableFunctions.OrderCreated
{
    public class FillOrderItemsDataActivityModel
    {
        public Order Order { get; set; }
        public IEnumerable<ProductDto> ValidatedProducts { get; set; }
    }
}
