﻿using Application.Customers;
using Application.Tables;

namespace Application.Orders.Queries.GetOrdersByCustomer
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Notes { get; set; }
    }

    public class OrderDto
    {
        public string Id { get; set; }
        public CustomerDto Customer { get; set; }
        public TableDto Table { get; set; }
        public int Amount { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime? ServedOn { get; set; }
        public DateTime? PaidOn { get; set; }
    }
}