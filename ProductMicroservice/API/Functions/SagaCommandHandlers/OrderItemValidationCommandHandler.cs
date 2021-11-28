using API.Constants;
using Application.Common.Interfaces;
using Eater.Shared.Common;
using Eater.Shared.Constants;
using Newtonsoft.Json;

namespace API.Functions.SagaCommandHandlers
{
    public class OrderItemValidationCommandHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;

        public OrderItemValidationCommandHandler(IProductRepository productRepository, ILogger<OrderItemValidationCommandHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [FunctionName("OrderItemValidationCommandHandler")]
        [return: ServiceBus(QueueNames.OrderOrchestrationEvent, Connection = AppSettingsKeys.ServiceBusConnString)]
        public async Task<EventEnvelope<OrderItemValidationEvent>> Run(
            [ServiceBusTrigger(QueueNames.OrderItemValidationCmd, Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            CancellationToken cancellationToken)
        {
            var order = JsonConvert.DeserializeObject<OrderCreatedEvent>(myQueueItem);

            var validationEventPayload = new EventEnvelope<OrderItemValidationEvent>(
                correlationId: order.Id,
                subject: nameof(Events.OrderItemValidation),
                eventType: Events.OrderItemValidation.OrderItemValidationSuccess,
                body: new OrderItemValidationEvent());

            try
            {
                var products = await _productRepository.GetAllByIdsAsync(order.Items.Select(item => item.Id).ToList(), cancellationToken);
                if (products.Count() != order.Items.Count)
                {
                    validationEventPayload.EventType = Events.OrderItemValidation.OrderItemValidationFailed;
                    validationEventPayload.Body.ErrorMessage = "One or more items are out of stock or disabled. Please try ordering again.";
                }
                else
                {
                    validationEventPayload.Body.Products = products;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught while validating order items: {ex}");

                validationEventPayload.EventType = Events.OrderItemValidation.OrderItemValidationFailed;
                validationEventPayload.Body.ErrorMessage = "An error occured while processing your order. Please try ordering again";
            }

            return validationEventPayload;
        }
    }
}
