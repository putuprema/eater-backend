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
            var input = context.GetInput<OrderDto>();

            var orderDb = await context.CallActivityWithRetryAsync<Order>(nameof(RefreshOrderDataActivity), retryOptions, input);
            await context.CallActivityWithRetryAsync(nameof(SendOrderCreatedEvent), retryOptions, orderDb);

            // Wait for order item validation
            var orderItemValidation = await context.WaitForExternalEvent<OrderItemValidationEventData>(OrderEvents.OrderItemValidationEvent);
            if (orderItemValidation.Error)
            {
                orderDb.Status = OrderStatus.CANCELED;
                orderDb.CancellationReason = orderItemValidation.ErrorMessage;

                LogOrchestration(orderDb, log, $"Order item validation failed ({orderItemValidation.ErrorMessage}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderStatusActivity), retryOptions, orderDb);
            }

            var orchestrationModel = new FillOrderItemsDataActivityModel { Order = orderDb, ValidatedProducts = orderItemValidation.Products };
            orderDb = await context.CallActivityWithRetryAsync<Order>(nameof(FillOrderItemsDataActivity), retryOptions, orchestrationModel);
            orderDb = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderStatusActivity), retryOptions, orderDb);

            LogOrchestration(orderDb, log, "Order item validated.");

            // Wait for payment event
            var paymentStatus = await context.WaitForExternalEvent<PaymentStatus>(OrderEvents.PaymentStatusChanged);
            orderDb.PaymentStatus = paymentStatus;

            // Payment rejected
            if (paymentStatus == PaymentStatus.REJECTED || paymentStatus == PaymentStatus.EXPIRED)
            {
                orderDb.Status = OrderStatus.CANCELED;
                orderDb.CancellationReason = paymentStatus switch
                {
                    PaymentStatus.REJECTED => "Payment rejected",
                    PaymentStatus.EXPIRED => "Payment expired",
                    _ => null
                };
                LogOrchestration(orderDb, log, $"Payment failed (Reason: {orderDb.CancellationReason}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderStatusActivity), retryOptions, orderDb);
            }

            // Payment accepted. Queue the order to the kitchen
            orderDb.PaidOn = DateTime.UtcNow;
            orderDb.Status = OrderStatus.QUEUED;
            orderDb = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderStatusActivity), retryOptions, orderDb);
            LogOrchestration(orderDb, log, "Payment received. Order has been queued to the kitchen.");

            // Order status tracking loop
            while (orderDb.Status != OrderStatus.COMPLETED && orderDb.Status != OrderStatus.CANCELED)
            {
                var newOrderStatus = await context.WaitForExternalEvent<OrderStatus>(OrderEvents.OrderStatusChanged);
                if (newOrderStatus - orderDb.Status == 1 || (orderDb.Status == OrderStatus.QUEUED && newOrderStatus == OrderStatus.CANCELED))
                {
                    orderDb.Status = newOrderStatus;
                    if (newOrderStatus == OrderStatus.SERVED)
                    {
                        orderDb.ServedOn = DateTime.UtcNow;
                    }
                    orderDb = await context.CallActivityWithRetryAsync<Order>(nameof(UpdateOrderStatusActivity), retryOptions, orderDb);
                    LogOrchestration(orderDb, log, "Order status updated.");
                }
            }

            LogOrchestration(orderDb, log, "Order workflow has been completed");
            return orderDb;
        }

        [FunctionName(nameof(RefreshOrderDataActivity))]
        public async Task<Order> RefreshOrderDataActivity([ActivityTrigger] OrderDto orderPayload, CancellationToken cancellationToken)
        {
            return await _orderRepository.GetByOrderIdAndCustomerIdAsync(orderPayload.Id, orderPayload.Customer.Id, cancellationToken);
        }

        [FunctionName(nameof(SendOrderCreatedEvent))]
        public async Task SendOrderCreatedEvent([ActivityTrigger] Order order,
            [EventGrid(TopicEndpointUri = AppSettingsKeys.EventGridTopicEndpointUri, TopicKeySetting = AppSettingsKeys.EventGridTopicKey)] IAsyncCollector<EventGridEvent> events,
            CancellationToken cancellationToken)
        {
            var eventPayload = JsonConvert.SerializeObject(order.Adapt<OrderDto>());
            await events.AddAsync(new EventGridEvent(Guid.NewGuid().ToString(),
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

        [FunctionName(nameof(UpdateOrderStatusActivity))]
        public async Task<Order> UpdateOrderStatusActivity(
            [ActivityTrigger] Order payload,
            [EventGrid(TopicEndpointUri = AppSettingsKeys.EventGridTopicEndpointUri, TopicKeySetting = AppSettingsKeys.EventGridTopicKey)] IAsyncCollector<EventGridEvent> events,
            CancellationToken cancellationToken)
        {
            var order = await _orderRepository.UpsertAsync(payload, cancellationToken);

            // Broadcast order status changed event
            var eventPayload = JsonConvert.SerializeObject(order.Adapt<OrderDto>());
            await events.AddAsync(new EventGridEvent(
                subject: Guid.NewGuid().ToString(),
                eventType: OrderEvents.OrderStatusChanged,
                data: new BinaryData(eventPayload),
                dataVersion: OrderEvents.OrderEventDataVersion), cancellationToken);

            return order;
        }
    }
}