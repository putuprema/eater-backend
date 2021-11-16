namespace Infrastructure.Repositories
{
    public class ActiveOrderRepository : IActiveOrderRepository
    {
        private readonly CosmosService _cosmosService;

        public ActiveOrderRepository(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cosmosService.Items.DeleteItemAsync<dynamic>(id, new PartitionKey(nameof(ActiveOrder)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
                    throw ex;
            }
        }

        public async Task<IEnumerable<ActiveOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var feedIterator = _cosmosService.Items.GetItemLinqQueryable<ActiveOrder>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(nameof(ActiveOrder)) })
                .OrderByDescending(s => s.CreatedOn)
                .ToFeedIterator();

            var sessions = new List<ActiveOrder>();
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                sessions.AddRange(response);
            }
            return sessions;
        }

        public async Task<ActiveOrder> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Items.ReadItemAsync<ActiveOrder>(id, new PartitionKey(nameof(ActiveOrder)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw ex;
            }
        }

        public async Task<ActiveOrder> UpsertAsync(ActiveOrder session, CancellationToken cancellationToken = default)
        {
            return await _cosmosService.Items.UpsertItemAsync(session, new PartitionKey(nameof(ActiveOrder)), cancellationToken: cancellationToken);
        }
    }
}
