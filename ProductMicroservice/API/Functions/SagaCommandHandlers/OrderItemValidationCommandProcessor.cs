using API.Constants;
using Application.Common.Interfaces;
using Newtonsoft.Json;

namespace API.Functions.SagaCommandHandlers
{
    public class OrderItemValidationCommandProcessor
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;

        public OrderItemValidationCommandProcessor(IProductRepository productRepository, ILogger<OrderItemValidationCommandProcessor> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [FunctionName("OrderItemValidationCommandProcessor")]
        [return: ServiceBus("order.saga.reply", Connection = AppSettingsKeys.ServiceBusConnString)]
        public async Task<string> Run(
            [ServiceBusTrigger("order.item.validation.cmd", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            CancellationToken cancellationToken)
        {
            var order = JsonConvert.DeserializeObject<OrderCreatedEvent>(myQueueItem);

            var validationEventPayload = new EventEnvelope<OrderItemValidationEvent>
            {
                CorrelationId = order.Id,
                EventType = Events.OrderItemValidationEvent,
                Body = new OrderItemValidationEvent()
            };

            try
            {
                var products = await _productRepository.GetAllByIdsAsync(order.Items.Select(item => item.Id).ToList(), cancellationToken);
                if (products.Count() != order.Items.Count)
                {
                    validationEventPayload.Success = false;
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

                validationEventPayload.Success = false;
                validationEventPayload.Body.ErrorMessage = "An error occured while processing your order. Please try ordering again";
            }

            return JsonConvert.SerializeObject(validationEventPayload);
        }
    }
}
