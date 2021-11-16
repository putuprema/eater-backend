using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Common.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByOrderIdAndCustomerIdAsync(string orderId, string customerId, CancellationToken cancellationToken = default);
        Task<PagedResultSet<Order>> GetByCustomerAsync(GetOrdersByCustomerQuery query, CancellationToken cancellationToken = default);
        Task<Order> UpsertAsync(Order order, CancellationToken cancellationToken = default);
    }
}
