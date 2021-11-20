using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Queries.GetActiveOrders
{
    public class GetActiveOrdersQueryHandler : IRequestHandler<GetActiveOrdersQuery, IEnumerable<OrderDto>>
    {
        private readonly IActiveOrderRepository _activeOrderRepository;

        public GetActiveOrdersQueryHandler(IActiveOrderRepository activeOrderRepository)
        {
            _activeOrderRepository = activeOrderRepository;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetActiveOrdersQuery request, CancellationToken cancellationToken)
        {
            var activeOrders = await _activeOrderRepository.GetAllAsync(cancellationToken);
            return activeOrders.Adapt<List<OrderDto>>();
        }
    }
}
