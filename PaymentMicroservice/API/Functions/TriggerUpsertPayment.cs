using Application.Payments.Query.GetPaymentInfo;
using Domain.Entities;
using Eater.Shared.Common;
using Eater.Shared.Constants;
using Mapster;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace API.Functions
{
    public static class TriggerUpsertPayment
    {
        [FunctionName(nameof(TriggerUpsertPayment))]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "PaymentMicroservice",
            collectionName: "Payments",
            ConnectionStringSetting = AppSettingsKeys.CosmosDbConnString,
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input,
            [ServiceBus(QueueNames.OrderOrchestrationEvent, Connection = AppSettingsKeys.ServiceBusConnString)] IAsyncCollector<EventEnvelope<PaymentDto>> events,
            ILogger log,
            CancellationToken cancellationToken)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var doc in input)
                {
                    log.LogInformation($"Payment Upsert: {doc}");
                    var payment = JsonConvert.DeserializeObject<Payment>(doc.ToString());

                    if (payment.Status != PaymentStatus.UNPAID)
                    {
                        var eventPayload = new EventEnvelope<PaymentDto>(
                            correlationId: payment.OrderId,
                            subject: nameof(Events.PaymentStatus),
                            eventType: Events.PaymentStatus.PaymentStatusChanged,
                            body: payment.Adapt<PaymentDto>());

                        await events.AddAsync(eventPayload, cancellationToken);
                    }
                }
            }
        }
    }
}
