using API.Functions.Orchestrations;
using Domain.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public class TriggerUpsertProducts
    {
        [FunctionName("TriggerUpsertProducts")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "ProductMicroservice",
            collectionName: "Items",
            ConnectionStringSetting = "CosmosConfig:ConnString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var doc in input)
                {
                    if (doc.GetPropertyValue<string>("objectType") == nameof(ProductCategory))
                    {
                        var productCategory = JsonConvert.DeserializeObject(doc.ToString());
                        await starter.StartNewAsync(nameof(CategoryDataUpdateOrchestration), productCategory);
                    }
                }
            }
        }
    }
}
