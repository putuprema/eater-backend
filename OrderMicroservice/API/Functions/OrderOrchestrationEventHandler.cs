using Eater.Shared.Common;
using Eater.Shared.Constants;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public static class OrderOrchestrationEventHandler
    {
        [FunctionName("OrderOrchestrationEventHandler")]
        public static async Task Run(
            [ServiceBusTrigger(QueueNames.OrderOrchestrationEvent, Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            var eventData = JsonConvert.DeserializeObject<EventEnvelope<dynamic>>(myQueueItem);
            await durableClient.RaiseEventAsync(eventData.CorrelationId, eventData.Subject, eventData);
        }
    }
}
