using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetActiveOrders
{
    public class GetActiveOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
    }
}
