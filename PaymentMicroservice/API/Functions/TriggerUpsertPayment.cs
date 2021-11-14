using Application.Payments.Query.GetPaymentInfo;
using Azure.Messaging.EventGrid;
using Domain.Entities;
using Mapster;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
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
            [EventGrid(TopicEndpointUri = AppSettingsKeys.EventGridTopicEndpointUri, TopicKeySetting = AppSettingsKeys.EventGridTopicKey)] IAsyncCollector<EventGridEvent> events,
            ILogger log,
            CancellationToken cancellationToken)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var doc in input)
                {
                    log.LogInformation($"Payment Upsert: {doc}");
                    var payment = JsonConvert.DeserializeObject<Payment>(doc.ToString());

                    await events.AddAsync(new EventGridEvent(Guid.NewGuid().ToString(),
                        PaymentEvents.EventTypePaymentStatusChanged,
                        PaymentEvents.PaymentEventDataVersion,
                        new BinaryData(payment.Adapt<PaymentDto>())), cancellationToken);
                }
            }
        }
    }
}
