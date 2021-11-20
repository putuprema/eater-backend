using Application.Orders.Queries.GetOrdersByCustomer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Orders.Queries.GetCustomerOrder
{
    public class GetCustomerOrderQuery : IRequest<OrderDto>
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
    }
}
