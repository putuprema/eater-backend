using API.Functions.Orchestrations;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public class TriggerUpsertTables
    {
        [FunctionName("TriggerUpsertTables")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "TableMicroservice",
            collectionName: "Items",
            ConnectionStringSetting = "CosmosConfig:ConnString",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)] IReadOnlyList<Document> input,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                foreach (var doc in input)
                {
                    if (doc.GetPropertyValue<string>("objectType") == nameof(Table))
                    {
                        var table = JsonConvert.DeserializeObject<Table>(doc.ToString());
                        if (table.IsNew)
                        {
                            log.LogInformation($"Generated table {table.Number} with ID: {table.Id}");
                            await starter.StartNewAsync(nameof(TableGenerationOrchestration), table);
                        }
                    }
                }
            }
        }
    }
}
