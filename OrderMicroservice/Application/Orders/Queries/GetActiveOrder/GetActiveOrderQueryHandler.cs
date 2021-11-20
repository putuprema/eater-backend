using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetActiveOrder
{
    public class GetActiveOrderQueryHandler : IRequestHandler<GetActiveOrderQuery, OrderDto>
    {
        private readonly IActiveOrderRepository _activeOrderRepository;

        public GetActiveOrderQueryHandler(IActiveOrderRepository activeOrderRepository)
        {
            _activeOrderRepository = activeOrderRepository;
        }

        public async Task<OrderDto> Handle(GetActiveOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _activeOrderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            return order.Adapt<OrderDto>();
        }
    }
}
