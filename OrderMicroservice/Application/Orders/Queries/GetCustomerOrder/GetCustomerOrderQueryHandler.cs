using Application.Orders.Queries.GetOrdersByCustomer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
