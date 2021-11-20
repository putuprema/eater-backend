using Application.Orders.Queries.GetOrdersByCustomer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Queries.GetActiveOrders
{
    public class GetActiveOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
    }
}
