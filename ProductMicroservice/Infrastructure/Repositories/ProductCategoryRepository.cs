namespace Infrastructure.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly CosmosService _cosmosService;
        private readonly IOptions<JsonSerializerSettings> _jsonSerializerSettings;
        private IEnumerable<ProductCategory> _categories;

        public ProductCategoryRepository(CosmosService cosmosService, IOptions<JsonSerializerSettings> jsonSerializerSettings)
        {
            _cosmosService = cosmosService;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cosmosService.Items.DeleteItemAsync<ProductCategory>(id, new PartitionKey(nameof(ProductCategory)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Product category not found");

                throw ex;
            }
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_categories == null)
            {
                _categories = await GetAllFromCosmosAsync(cancellationToken);
            }
            return _categories;
        }

        private async Task<IEnumerable<ProductCategory>> GetAllFromCosmosAsync(CancellationToken cancellationToken = default)
        {
            using var feedIterator = _cosmosService.Items.GetItemLinqQueryable<ProductCategory>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(nameof(ProductCategory))
            })
                .OrderBy(c => c.SortIndex)
                .ThenBy(c => c.Name)
                .ToFeedIterator();

            var categories = new List<ProductCategory>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                categories.AddRange(response);
            }

            return categories;
        }

        public async Task<ProductCategory> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Items.ReadItemAsync<ProductCategory>(id, new PartitionKey(nameof(ProductCategory)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<ProductCategory> UpsertAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
        {
            var payload = JsonConvert.SerializeObject(productCategory, _jsonSerializerSettings.Value);

            var result = await _cosmosService.Items.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<ProductCategory>>(
                StoredProcedure.UpsertProductCategory,
                new PartitionKey(nameof(ProductCategory)),
                parameters: new[] { payload },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }

        public async Task<int> PopulateCategoryCacheAsync(CancellationToken cancellationToken = default)
        {
            _categories = await GetAllFromCosmosAsync(cancellationToken);
            return _categories.Count();
        }
    }
}
