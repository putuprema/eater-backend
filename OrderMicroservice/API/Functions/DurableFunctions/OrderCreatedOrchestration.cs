using Application.Common.Interfaces;
using Application.Orders.Commands;
using Application.Orders.Queries.GetOrdersByCustomer;
using Domain.Entities;
using Eater.Shared.Common;
using Eater.Shared.Constants;
using Infrastructure.Config;
using Mapster;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.DurableFunctions
{
    public class OrderCreatedOrchestration
    {
        private readonly IOrderRepository _orderRepository;
        private readonly DurableFunctionConfig _durableFunctionConfig;
        private readonly IActiveOrderRepository _activeOrderRepository;

        public OrderCreatedOrchestration(IOrderRepository orderRepository, IOptions<DurableFunctionConfig> durableFunctionConfig, IActiveOrderRepository activeOrderRepository)
        {
            _orderRepository = orderRepository;
            _durableFunctionConfig = durableFunctionConfig.Value;
            _activeOrderRepository = activeOrderRepository;
        }

        private static void LogOrchestration(Order order, ILogger log, string message)
        {
            log.LogInformation($"[Order Orchestration (Id = {order.Id}, Status = {order.Status})] => {message}");
        }

        [FunctionName(Orchestrations.OrderOrchestration)]
        public async Task<Order> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
        {
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var log = context.CreateReplaySafeLogger(logger);

            var orderData = context.GetInput<Order>();
            LogOrchestration(orderData, log, "Order workflow started.");

            await context.CallActivityWithRetryAsync(nameof(ProduceOrderItemValidationCommand), retryOptions, orderData);

            // Wait for order item validation
            var orderItemValidation = await context.WaitForExternalEvent<EventEnvelope<OrderItemValidationEvent>>(nameof(Events.OrderItemValidation));
            if (orderItemValidation.EventType != Events.OrderItemValidation.OrderItemValidationSuccess)
            {
                orderData.Status = OrderStatus.CANCELED;
                orderData.CancellationReason = orderItemValidation.Body.ErrorMessage;

                LogOrchestration(orderData, log, $"Order item validation failed ({orderData.CancellationReason}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            }

            var orchestrationModel = new FillOrderItemsDataActivityModel { Order = orderData, ValidatedProducts = orderItemValidation.Body.Products };
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(FillOrderItemsDataActivity), retryOptions, orchestrationModel);
            LogOrchestration(orderData, log, "Order item validated.");

            await context.CallActivityWithRetryAsync(nameof(ProduceInitPaymentCommand), retryOptions, orderData);

            // Wait for init payment event
            var initPaymentEvent = await context.WaitForExternalEvent<EventEnvelope<Payment>>(nameof(Events.InitPayment));
            if (initPaymentEvent.EventType != Events.InitPayment.InitPaymentSuccess)
            {
                orderData.Status = OrderStatus.CANCELED;
                orderData.CancellationReason = "Init payment failed";

                LogOrchestration(orderData, log, $"Order init payment failed. Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            }

            orderData.Status = OrderStatus.PENDING_PAYMENT;
            orderData.Payment = initPaymentEvent.Body;
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            LogOrchestration(orderData, log, "Order payment init success. Waiting for payment.");

            // Wait for payment status event
            var paymentStatusEvent = await context.WaitForExternalEvent<EventEnvelope<Payment>>(nameof(Events.PaymentStatus));
            orderData.Payment = paymentStatusEvent.Body;

            // Payment rejected
            if (orderData.Payment.Status == PaymentStatus.REJECTED || orderData.Payment.Status == PaymentStatus.EXPIRED)
            {
                orderData.Status = OrderStatus.CANCELED;
                orderData.CancellationReason = orderData.Payment.Status switch
                {
                    PaymentStatus.REJECTED => "Payment rejected",
                    PaymentStatus.EXPIRED => "Payment expired",
                    _ => null
                };
                LogOrchestration(orderData, log, $"Payment failed (Reason: {orderData.CancellationReason}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            }

            // Payment accepted. Queue the order to the kitchen
            orderData.Status = OrderStatus.QUEUED;
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            LogOrchestration(orderData, log, "Payment received. Order has been queued to the kitchen.");

            // Order status tracking loop
            while (orderData.Status != OrderStatus.SERVED && orderData.Status != OrderStatus.CANCELED)
            {
                var newOrderStatus = await context.WaitForExternalEvent<OrderStatus>(Events.OrderStatus.OrderStatusChanged);
                if (newOrderStatus - orderData.Status == 1 || (orderData.Status == OrderStatus.QUEUED && newOrderStatus == OrderStatus.CANCELED))
                {
                    orderData.Status = newOrderStatus;
                    if (newOrderStatus == OrderStatus.SERVED)
                    {
                        orderData.ServedOn = context.CurrentUtcDateTime;
                    }
                    orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
                    LogOrchestration(orderData, log, "Order status updated.");
                }
            }

            LogOrchestration(orderData, log, "Order workflow has been completed");
            return orderData;
        }

        [FunctionName(nameof(ProduceOrderItemValidationCommand))]
        public async Task ProduceOrderItemValidationCommand([ActivityTrigger] Order order,
            [ServiceBus(QueueNames.OrderItemValidationCmd, Connection = AppSettingsKeys.ServiceBusConnString)] IAsyncCollector<OrderDto> commands)
        {
            await commands.AddAsync(order.Adapt<OrderDto>());
        }

        [FunctionName(nameof(ProduceInitPaymentCommand))]
        public async Task ProduceInitPaymentCommand([ActivityTrigger] Order order,
            [ServiceBus(QueueNames.InitPaymentCmd, Connection = AppSettingsKeys.ServiceBusConnString)] IAsyncCollector<InitPaymentCommand> commands)
        {
            await commands.AddAsync(new InitPaymentCommand(order));
        }

        [FunctionName(nameof(FillOrderItemsDataActivity))]
        public Order FillOrderItemsDataActivity([ActivityTrigger] FillOrderItemsDataActivityModel payload)
        {
            var order = payload.Order;
            var validatedProducts = payload.ValidatedProducts.ToDictionary(x => x.Id);

            var totalAmount = 0;

            order.Items = order.Items.Select(item =>
            {
                var product = validatedProducts[item.Id];

                item.Name = product.Name;
                item.Price = product.Price;
                item.ImageUrl = product.ImageUrl;

                totalAmount += (item.Price * item.Quantity);

                return item;
            }).ToList();

            order.Amount = totalAmount;
            return order;
        }

        [FunctionName(nameof(UpdateOrderActivity))]
        public async Task<Order> UpdateOrderActivity([ActivityTrigger] Order payload, CancellationToken cancellationToken)
        {
            payload.IsNew = false;
            return await _orderRepository.UpsertAsync(payload, cancellationToken);
        }
    }

    public class FillOrderItemsDataActivityModel
    {
        public Order Order { get; set; }
        public IEnumerable<OrderItem> ValidatedProducts { get; set; }
    }
}