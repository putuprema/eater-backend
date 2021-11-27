using Domain.Entities;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public static class OrderEventHandlers
    {
        [FunctionName("OrderSagaReplyEventHandler")]
        public static async Task HandleOrderSageReplyEvent(
            [ServiceBusTrigger("order.saga.reply", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            var eventData = JsonConvert.DeserializeObject<EventEnvelope<dynamic>>(myQueueItem);
            await durableClient.RaiseEventAsync(eventData.CorrelationId, eventData.EventType, eventData);
        }

        [FunctionName("PaymentEventHandler")]
        public static async Task HandlePaymentEvent(
            [ServiceBusTrigger("order.payment.event", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            var paymentEvt = JsonConvert.DeserializeObject<EventEnvelope<Payment>>(myQueueItem);
            await durableClient.RaiseEventAsync(paymentEvt.CorrelationId, Events.PaymentStatusChanged, paymentEvt.Body);
        }
    }
}
