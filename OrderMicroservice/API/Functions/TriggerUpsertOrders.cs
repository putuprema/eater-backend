using Application.Common.Interfaces;
using Domain.Entities;
using Mapster;
using Microsoft.Azure.Documents;
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
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input)
        {
            if (input != null && input.Count > 0)
            {
                var tasks = new List<Task>();

                foreach (var doc in input)
                {
                    var order = JsonConvert.DeserializeObject<Order>(doc.ToString());

                    // If order is active, replicate changes to the active order container
                    if (order.Status != OrderStatus.VALIDATING && order.Status != OrderStatus.PENDING_PAYMENT)
                    {
                        var activeOrder = order.Adapt<ActiveOrder>();
                        if (activeOrder.Status == OrderStatus.COMPLETED || activeOrder.Status == OrderStatus.CANCELED)
                        {
                            activeOrder.MarkForDeletion();
                        }
                        tasks.Add(_activeOrderRepository.UpsertAsync(activeOrder));
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
