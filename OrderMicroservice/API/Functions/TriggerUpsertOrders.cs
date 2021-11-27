using Application.Common.Interfaces;
using Application.Orders.Queries.GetOrdersByCustomer;
using Azure.Messaging.EventGrid;
using Domain.Entities;
using Mapster;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json;

namespace API.Functions
{
    public class TriggerUpsertOrders
    {
        private readonly IActiveOrderRepository _activeOrderRepository;

        public TriggerUpsertOrders(IActiveOrderRepository activeOrderRepository)
        {
            _activeOrderRepository = activeOrderRepository;
        }

        [FunctionName("TriggerUpsertOrders")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "OrderMicroservice",
            collectionName: "Orders",
            ConnectionStringSetting = AppSettingsKeys.CosmosDbConnString,
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input,
            [DurableClient] IDurableOrchestrationClient starter,
            [EventGrid(TopicEndpointUri = AppSettingsKeys.EventGridTopicEndpointUri, TopicKeySetting = AppSettingsKeys.EventGridTopicKey)] IAsyncCollector<EventGridEvent> events)
        {
            if (input != null && input.Count > 0)
            {
                var tasks = new List<Task>();

                foreach (var doc in input)
                {
                    var order = JsonConvert.DeserializeObject<Order>(doc.ToString());

                    // Start order orchestration for newly created orders
                    if (order.IsNew)
                    {
                        tasks.Add(starter.StartNewAsync(Orchestrations.OrderOrchestration, order.Id, order));
                    }
                    else
                    {
                        // Publish order status changed event
                        var orderDtoPayload = JsonConvert.SerializeObject(order.Adapt<OrderDto>());
                        await events.AddAsync(new EventGridEvent(
                            subject: order.Id,
                            eventType: Events.OrderStatusChanged,
                            data: new BinaryData(orderDtoPayload),
                            dataVersion: Events.EventDataVersion));

                        // If order is active, replicate changes to the active order container
                        if (order.Status != OrderStatus.VALIDATING && order.Status != OrderStatus.PENDING_PAYMENT)
                        {
                            var activeOrder = order.Adapt<ActiveOrder>();
                            if (activeOrder.Status == OrderStatus.SERVED || activeOrder.Status == OrderStatus.CANCELED)
                            {
                                activeOrder.MarkForDeletion();
                            }
                            tasks.Add(_activeOrderRepository.UpsertAsync(activeOrder));
                        }
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
