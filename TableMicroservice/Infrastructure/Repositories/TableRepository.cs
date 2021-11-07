namespace Infrastructure.Repositories
{
    public class TableRepository : ITableRepository
    {
        private readonly CosmosService _cosmosService;

        public TableRepository(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task GenerateTablesAsync(int quantity, CancellationToken cancellationToken = default)
        {
            var result = await _cosmosService.Items.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<dynamic>>(
                StoredProcedure.GenerateTables,
                new PartitionKey(nameof(Table)),
                parameters: new dynamic[] { quantity },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<Table>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var feedIterator = _cosmosService.Items.GetItemLinqQueryable<Table>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(nameof(Table))
            })
                .OrderBy(c => c.Number)
                .ToFeedIterator();

            var tables = new List<Table>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                tables.AddRange(response);
            }

            return tables;
        }

        public async Task<Table> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Items.ReadItemAsync<Table>(id, new PartitionKey(nameof(Table)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<Table> SetActiveAsync(string tableId, bool active, CancellationToken cancellationToken = default)
        {
            var result = await _cosmosService.Items.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<Table>>(
                StoredProcedure.SetActive,
                new PartitionKey(nameof(Table)),
                parameters: new dynamic[] { tableId, active },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }

        public async Task<Table> UpsertAsync(Table table, CancellationToken cancellationToken = default)
        {
            return await _cosmosService.Items.UpsertItemAsync(table, new PartitionKey(nameof(Table)), cancellationToken: cancellationToken);
        }
    }
}
