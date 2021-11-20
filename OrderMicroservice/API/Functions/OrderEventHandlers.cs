using Application.Orders.Events;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public static class OrderEventHandlers
    {
        [FunctionName("OrderItemValidationEventHandler")]
        public static async Task HandleOrderItemValidationEvent(
            [ServiceBusTrigger("order.item.validation", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            var eventData = JsonConvert.DeserializeObject<OrderItemValidationEvent>(myQueueItem);
            await durableClient.RaiseEventAsync(eventData.OrderId, OrderEvents.OrderItemValidationEvent, eventData);
        }

        [FunctionName("PaymentEventHandler")]
        public static async Task HandlePaymentEvent(
            [ServiceBusTrigger("order.payment.event", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            var eventData = EventGridEvent.Parse(new BinaryData(myQueueItem));
            var payment = JsonConvert.DeserializeObject<PaymentEvent>(eventData.Data.ToString());
            await durableClient.RaiseEventAsync(payment.OrderId, OrderEvents.PaymentStatusChanged, payment.Status);
        }
    }
}
