using Application.Common.Interfaces;
using Application.Orders.Queries.GetOrdersByCustomer;
using Azure.Messaging.EventGrid;
using Domain.Entities;
using Infrastructure.Config;
using Mapster;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Functions.DurableFunctions.OrderCreated
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

            await context.CallActivityWithRetryAsync(nameof(PublishOrderCreatedEvent), retryOptions, orderData);

            // Wait for order item validation
            var orderItemValidation = await context.WaitForExternalEvent<OrderItemValidationEventData>(OrderEvents.OrderItemValidationEvent);
            if (orderItemValidation.Error)
            {
                orderData.Status = OrderStatus.CANCELED;
                orderData.CancellationReason = orderItemValidation.ErrorMessage;

                LogOrchestration(orderData, log, $"Order item validation failed ({orderItemValidation.ErrorMessage}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            }

            var orchestrationModel = new FillOrderItemsDataActivityModel { Order = orderData, ValidatedProducts = orderItemValidation.Products };
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(FillOrderItemsDataActivity), retryOptions, orchestrationModel);
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);

            LogOrchestration(orderData, log, "Order item validated.");

            // Wait for payment event
            var paymentStatus = await context.WaitForExternalEvent<PaymentStatus>(OrderEvents.PaymentStatusChanged);
            orderData.PaymentStatus = paymentStatus;

            // Payment rejected
            if (paymentStatus == PaymentStatus.REJECTED || paymentStatus == PaymentStatus.EXPIRED)
            {
                orderData.Status = OrderStatus.CANCELED;
                orderData.CancellationReason = paymentStatus switch
                {
                    PaymentStatus.REJECTED => "Payment rejected",
                    PaymentStatus.EXPIRED => "Payment expired",
                    _ => null
                };
                LogOrchestration(orderData, log, $"Payment failed (Reason: {orderData.CancellationReason}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            }

            // Payment accepted. Queue the order to the kitchen
            orderData.PaidOn = DateTime.UtcNow;
            orderData.Status = OrderStatus.QUEUED;
            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
            LogOrchestration(orderData, log, "Payment received. Order has been queued to the kitchen.");

            // Order status tracking loop
            while (orderData.Status != OrderStatus.COMPLETED && orderData.Status != OrderStatus.CANCELED)
            {
                var newOrderStatus = orderData.Status == OrderStatus.SERVED ?
                    await context.WaitForExternalEvent(OrderEvents.OrderStatusChanged, TimeSpan.FromHours(2), OrderStatus.COMPLETED) :
                    await context.WaitForExternalEvent<OrderStatus>(OrderEvents.OrderStatusChanged);

                if (newOrderStatus - orderData.Status == 1 || (orderData.Status == OrderStatus.QUEUED && newOrderStatus == OrderStatus.CANCELED))
                {
                    orderData.Status = newOrderStatus;
                    if (newOrderStatus == OrderStatus.SERVED)
                    {
                        orderData.ServedOn = DateTime.UtcNow;
                    }
                    orderData = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderActivity), retryOptions, orderData);
                    LogOrchestration(orderData, log, "Order status updated.");
                }
            }

            LogOrchestration(orderData, log, "Order workflow has been completed");
            return orderData;
        }

        [FunctionName(nameof(PublishOrderCreatedEvent))]
        public async Task PublishOrderCreatedEvent([ActivityTrigger] Order order,
            [EventGrid(TopicEndpointUri = AppSettingsKeys.EventGridTopicEndpointUri, TopicKeySetting = AppSettingsKeys.EventGridTopicKey)] IAsyncCollector<EventGridEvent> events,
            CancellationToken cancellationToken)
        {
            var eventPayload = JsonConvert.SerializeObject(order.Adapt<OrderDto>());
            await events.AddAsync(new EventGridEvent(order.Id,
                OrderEvents.OrderCreated,
                OrderEvents.OrderEventDataVersion,
                new BinaryData(eventPayload)), cancellationToken);
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
            order.Status = OrderStatus.PENDING_PAYMENT;
            return order;
        }

        [FunctionName(nameof(UpdateOrderActivity))]
        public async Task<Order> UpdateOrderActivity([ActivityTrigger] Order payload, CancellationToken cancellationToken)
        {
            payload.IsNew = false;
            return await _orderRepository.UpsertAsync(payload, cancellationToken);
        }
    }
}