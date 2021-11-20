using Application.Orders.Queries.GetOrdersByCustomer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Queries.GetActiveOrder
{
    public class GetActiveOrderQuery : IRequest<OrderDto>
    {
        public string OrderId { get; set; }
    }
}
