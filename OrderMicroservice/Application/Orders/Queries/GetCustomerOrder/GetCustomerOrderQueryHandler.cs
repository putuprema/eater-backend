using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetCustomerOrder
{
    public class GetCustomerOrderQueryHandler : IRequestHandler<GetCustomerOrderQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;

        public GetCustomerOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto> Handle(GetCustomerOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByOrderIdAndCustomerIdAsync(request.OrderId, request.CustomerId, cancellationToken);
            return order.Adapt<OrderDto>();
        }
    }
}
