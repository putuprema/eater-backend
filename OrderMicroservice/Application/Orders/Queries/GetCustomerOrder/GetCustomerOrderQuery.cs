using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetCustomerOrder
{
    public class GetCustomerOrderQuery : IRequest<OrderDto>
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
    }
}
