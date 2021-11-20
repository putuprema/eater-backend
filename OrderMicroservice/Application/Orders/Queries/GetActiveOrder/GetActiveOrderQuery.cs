using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetActiveOrder
{
    public class GetActiveOrderQuery : IRequest<OrderDto>
    {
        public string OrderId { get; set; }
    }
}
