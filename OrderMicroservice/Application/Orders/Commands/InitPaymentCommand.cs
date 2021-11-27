namespace Application.Orders.Commands
{
    public class PaymentItem
    {
        public string Name { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }

    public class InitPaymentCommand
    {
        public string CustomerEmail { get; set; }
        public string OrderId { get; set; }
        public List<PaymentItem> Items { get; set; }

        public InitPaymentCommand(Order order)
        {
            CustomerEmail = order.Customer.Email;
            OrderId = order.Id;
            Items = order.Items.Select(i => new PaymentItem
            {
                Name = i.Name,
                Image = i.ImageUrl,
                UnitPrice = i.Price,
                Quantity = i.Quantity
            }).ToList();
        }
    }
}
