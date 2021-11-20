using API.Constants;
using Application.Common.Interfaces;
using Application.Orders;
using Application.Products.Queries.GetProducts;
using Azure.Messaging.EventGrid;
using Mapster;
using Newtonsoft.Json;

namespace API.Functions.EventHandlers
{
    internal class OrderItemValidationEventData
    {
        public string OrderId { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }

    public class OrderCreatedEventHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger _logger;

        public OrderCreatedEventHandler(IProductRepository productRepository, ILogger<OrderCreatedEventHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [FunctionName("OrderCreatedEventHandler")]
        [return: ServiceBus("order.item.validation", Connection = AppSettingsKeys.ServiceBusConnString)]
        public async Task<string> Run(
            [ServiceBusTrigger("order.created.evt.to.productsvc", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            CancellationToken cancellationToken)
        {
            var eventData = EventGridEvent.Parse(new BinaryData(myQueueItem));
            var order = JsonConvert.DeserializeObject<OrderDto>(eventData.Data.ToString());

            var validationEventPayload = new OrderItemValidationEventData { OrderId = order.Id };
            try
            {
                var products = await _productRepository.GetAllByIdsAsync(order.Items.Select(item => item.Id).ToList(), cancellationToken);
                if (products.Count() != order.Items.Count)
                {
                    validationEventPayload.Error = true;
                    validationEventPayload.ErrorMessage = "One or more items are out of stock or disabled. Please try ordering again.";
                }
                else
                {
                    validationEventPayload.Products = products.Adapt<List<ProductDto>>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught while validating order items: {ex}");

                validationEventPayload.Error = true;
                validationEventPayload.ErrorMessage = "An error occured while processing your order. Please try ordering again";
            }

            return JsonConvert.SerializeObject(validationEventPayload);
        }
    }
}
