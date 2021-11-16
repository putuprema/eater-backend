using Application.Orders.Queries.GetOrdersByCustomer;

namespace Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderItems = request.Items.Select(i => new OrderItem
            {
                Id = i.Id,
                Quantity = i.Quantity,
                Notes = i.Notes
            }).ToList();

            var order = new Order
            {
                Customer = new Customer { Id = request.Customer.Id, Name = request.Customer.Name, Email = request.Customer.Email },
                Table = new Table { Id = request.Table.Id, Number = request.Table.Number },
                Items = orderItems
            };

            order = await _orderRepository.UpsertAsync(order, cancellationToken);
            return order.Adapt<OrderDto>();
        }
    }
}
