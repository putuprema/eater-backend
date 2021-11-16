namespace Application.Orders.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerQuery : IRequest<PagedResultSet<OrderDto>>
    {
        public string CustomerId { get; set; }
        public int PageSize { get; set; } = 10;
        public string ContinuationToken { get; set; }
    }
}
