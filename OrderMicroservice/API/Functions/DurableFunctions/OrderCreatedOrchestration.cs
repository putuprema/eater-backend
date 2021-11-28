using Application.Common.Interfaces;
using Application.Orders.Commands;
using Application.Orders.Queries.GetOrdersByCustomer;
using Domain.Entities;
using Eater.Shared.Constants;
using Infrastructure.Config;
using Mapster;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.DurableFunctions
{
    public record FillOrderItemsDataActivityModel(Order Order, IEnumerable<OrderItem> ValidatedProducts);
    public record CancelOrderActivityModel(Order Order, string CancellationReason);
    public record HandlePaymentActivityModel(Order Order, Payment Payment);
    public record HandleNewOrderStatusActivityModel(Order Order, OrderStatus NewStatus);

    public class OrderCreatedOrchestration
    {
        private readonly IOrderRepository _orderRepository;
        private readonly DurableFunctionConfig _durableFunctionConfig;
        private readonly RetryOptions _retryOptions;

        public OrderCreatedOrchestration(IOrderRepository orderRepository, IOptions<DurableFunctionConfig> durableFunctionConfig)
        {
            _orderRepository = orderRepository;
            _durableFunctionConfig = durableFunctionConfig.Value;

            _retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);
        }

        private static void LogOrchestration(Order order, ILogger log, string message)
        {
            log.LogInformation($"[Order Orchestration (Id = {order.Id}, Status = {order.Status})] => {message}");
        }

        [FunctionName(Orchestrations.OrderOrchestration)]
        public async Task<Order> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
        {
            var log = context.CreateReplaySafeLogger(logger);

            var orderData = context.GetInput<Order>();
            LogOrchestration(orderData, log, "Order workflow started.");

            // Publish order item validation command and wait for the result
            await context.CallActivityWithRetryAsync(nameof(PublishOrderItemValidationCommand), _retryOptions, orderData);

            var validationEvent = await context.WaitForExternalEvent<EventEnvelope<OrderItemValidationEvent>>(nameof(Events.OrderItemValidation));
            if (validationEvent.EventType != Events.OrderItemValidation.OrderItemValidationSuccess)
            {
                var errorMessage = validationEvent.Body.ErrorMessage;
                LogOrchestration(orderData, log, $"Order item validation failed ({errorMessage}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(CancelOrderActivity), _retryOptions, new CancelOrderActivityModel(orderData, errorMessage));
            }

            orderData = await context.CallActivityAsync<Order>(nameof(HandleSuccessItemValidationActivity), new FillOrderItemsDataActivityModel(orderData, validationEvent.Body.Products));
            LogOrchestration(orderData, log, "Order item validated.");

            // Publish init payment command and wait for the result
            await context.CallActivityWithRetryAsync(nameof(PublishInitPaymentCommand), _retryOptions, orderData);

            var initPaymentEvent = await context.WaitForExternalEvent<EventEnvelope<Payment>>(nameof(Events.InitPayment));
            if (initPaymentEvent.EventType != Events.InitPayment.InitPaymentSuccess)
            {
                LogOrchestration(orderData, log, $"Order init payment failed. Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(CancelOrderActivity), _retryOptions, new CancelOrderActivityModel(orderData, "Initialize payment failed"));
            }

            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(HandleSuccessInitPaymentActivity), _retryOptions, new HandlePaymentActivityModel(orderData, initPaymentEvent.Body));
            LogOrchestration(orderData, log, "Order payment init success. Waiting for payment.");

            // Wait for payment status event
            var paymentStatusEvent = await context.WaitForExternalEvent<EventEnvelope<Payment>>(nameof(Events.PaymentStatus));
            var paymentStatusEventBody = paymentStatusEvent.Body;

            if (orderData.Payment.Status == PaymentStatus.REJECTED || orderData.Payment.Status == PaymentStatus.EXPIRED)
            {
                LogOrchestration(orderData, log, $"Payment failed (Status: {paymentStatusEventBody.Status}). Cancelling order...");
                return await context.CallActivityWithRetryAsync<Order>(nameof(HandleAbortedPaymentActivity), _retryOptions, new HandlePaymentActivityModel(orderData, paymentStatusEventBody));
            }

            orderData = await context.CallActivityWithRetryAsync<Order>(nameof(HandleSuccessPaymentActivity), _retryOptions, new HandlePaymentActivityModel(orderData, paymentStatusEventBody));
            LogOrchestration(orderData, log, "Payment received. Order has been queued to the kitchen.");

            // Keep looping while order is still active
            while (orderData.Status != OrderStatus.SERVED && orderData.Status != OrderStatus.CANCELED)
            {
                // Wait for order status changed event
                var newOrderStatus = await context.WaitForExternalEvent<OrderStatus>(Events.OrderStatus.OrderStatusChanged);
                orderData = await context.CallActivityWithRetryAsync<Order>(nameof(HandleNewOrderStatusActivity), _retryOptions, new HandleNewOrderStatusActivityModel(orderData, newOrderStatus));
                LogOrchestration(orderData, log, "Order status updated.");
            }

            LogOrchestration(orderData, log, "Order workflow has been completed");
            return orderData;
        }

        [FunctionName(nameof(PublishOrderItemValidationCommand))]
        public async Task PublishOrderItemValidationCommand([ActivityTrigger] Order order,
            [ServiceBus(QueueNames.OrderItemValidationCmd, Connection = AppSettingsKeys.ServiceBusConnString)] IAsyncCollector<OrderDto> commands)
        {
            await commands.AddAsync(order.Adapt<OrderDto>());
        }

        [FunctionName(nameof(PublishInitPaymentCommand))]
        public async Task PublishInitPaymentCommand([ActivityTrigger] Order order,
            [ServiceBus(QueueNames.InitPaymentCmd, Connection = AppSettingsKeys.ServiceBusConnString)] IAsyncCollector<InitPaymentCommand> commands)
        {
            await commands.AddAsync(new InitPaymentCommand(order));
        }

        [FunctionName(nameof(HandleSuccessItemValidationActivity))]
        public Order HandleSuccessItemValidationActivity([ActivityTrigger] FillOrderItemsDataActivityModel payload)
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

        [FunctionName(nameof(HandleSuccessInitPaymentActivity))]
        public async Task<Order> HandleSuccessInitPaymentActivity([ActivityTrigger] HandlePaymentActivityModel model, CancellationToken cancellationToken)
        {
            model.Order.Status = OrderStatus.PENDING_PAYMENT;
            model.Order.Payment = model.Payment;
            return await UpdateOrderActivity(model.Order, cancellationToken);
        }

        [FunctionName(nameof(HandleAbortedPaymentActivity))]
        public async Task<Order> HandleAbortedPaymentActivity([ActivityTrigger] HandlePaymentActivityModel model, CancellationToken cancellationToken)
        {
            var order = model.Order;

            order.Payment = model.Payment;
            order.Status = OrderStatus.CANCELED;
            order.CancellationReason = order.Payment.Status switch
            {
                PaymentStatus.REJECTED => "Payment rejected",
                PaymentStatus.EXPIRED => "Payment expired",
                _ => null
            };

            return await UpdateOrderActivity(order, cancellationToken);
        }

        [FunctionName(nameof(HandleSuccessPaymentActivity))]
        public async Task<Order> HandleSuccessPaymentActivity([ActivityTrigger] HandlePaymentActivityModel model, CancellationToken cancellationToken)
        {
            model.Order.Payment = model.Payment;
            model.Order.Status = OrderStatus.QUEUED;
            return await UpdateOrderActivity(model.Order, cancellationToken);
        }

        [FunctionName(nameof(HandleNewOrderStatusActivity))]
        public async Task<Order> HandleNewOrderStatusActivity([ActivityTrigger] HandleNewOrderStatusActivityModel model, CancellationToken cancellationToken)
        {
            var orderData = model.Order;
            var newOrderStatus = model.NewStatus;

            if (newOrderStatus - orderData.Status == 1 || (orderData.Status == OrderStatus.QUEUED && newOrderStatus == OrderStatus.CANCELED))
            {
                orderData.Status = newOrderStatus;
                if (newOrderStatus == OrderStatus.SERVED)
                {
                    orderData.ServedOn = DateTime.UtcNow;
                }
                orderData = await UpdateOrderActivity(orderData, cancellationToken);
            }

            return orderData;
        }

        [FunctionName(nameof(CancelOrderActivity))]
        public async Task<Order> CancelOrderActivity([ActivityTrigger] CancelOrderActivityModel model, CancellationToken cancellationToken)
        {
            model.Order.Status = OrderStatus.CANCELED;
            model.Order.CancellationReason = model.CancellationReason;
            return await UpdateOrderActivity(model.Order, cancellationToken);
        }

        [FunctionName(nameof(UpdateOrderActivity))]
        public async Task<Order> UpdateOrderActivity([ActivityTrigger] Order payload, CancellationToken cancellationToken)
        {
            payload.IsNew = false;
            return await _orderRepository.UpsertAsync(payload, cancellationToken);
        }
    }
}