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
                    var objectType = doc.GetPropertyValue<string>("objectType");

                    if (objectType == nameof(ProductCategory))
                    {
                        var productCategory = JsonConvert.DeserializeObject<ProductCategory>(doc.ToString());
                        await starter.StartNewAsync(nameof(ProductCategoryUpdateOrchestration), productCategory);
                    }
                    else if (objectType == nameof(Product))
                    {
                        var product = JsonConvert.DeserializeObject<Product>(doc.ToString());
                        if (product.Deleted)
                        {
                            await starter.StartNewAsync(nameof(ProductCleanupOrchestration), product);
                        }
                        await starter.StartNewAsync(nameof(ProductUpsertOrchestration), product);
                    }
                }
            }
        }
    }
}
