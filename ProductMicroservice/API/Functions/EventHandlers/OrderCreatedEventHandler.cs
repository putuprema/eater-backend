using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Application.Products.Queries.GetProducts;
using Newtonsoft.Json;
using Application.Orders;
using Application.Common.Interfaces;
using Mapster;
using API.Constants;
using Azure.Messaging.EventGrid;

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

        public OrderCreatedEventHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [FunctionName("OrderCreatedEventHandler")]
        [return: ServiceBus("order.item.validation", Connection = AppSettingsKeys.ServiceBusConnString)]
        public async Task<string> Run(
            [ServiceBusTrigger("order.created.evt.to.productsvc", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem, 
            CancellationToken cancellationToken)
        {
            var eventData = EventGridEvent.Parse(new BinaryData(myQueueItem));
            var order = JsonConvert.DeserializeObject<OrderDto>(eventData.Data.ToString());
            var products = await _productRepository.GetAllByIdsAsync(order.Items.Select(item => item.Id).ToList(), cancellationToken);

            var validationEventPayload = new OrderItemValidationEventData { OrderId = order.Id };

            if (products.Count() != order.Items.Count)
            {
                validationEventPayload.Error = true;
                validationEventPayload.ErrorMessage = "One or more items are out of stock or disabled. Please try ordering again.";
            }
            else
            {
                validationEventPayload.Products = products.Adapt<List<ProductDto>>();
            }

            return JsonConvert.SerializeObject(validationEventPayload);
        }
    }
}
