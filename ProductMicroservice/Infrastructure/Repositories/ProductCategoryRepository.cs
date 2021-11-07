namespace Infrastructure.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly Container _container;
        private readonly IOptions<JsonSerializerSettings> _jsonSerializerSettings;

        public ProductCategoryRepository(CosmosService cosmosService, IOptions<JsonSerializerSettings> jsonSerializerSettings)
        {
            _container = cosmosService.GetContainer("Items");
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                await _container.DeleteItemAsync<ProductCategory>(id, new PartitionKey(nameof(ProductCategory)), cancellationToken: cancellationToken);
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
            using var feedIterator = _container.GetItemLinqQueryable<ProductCategory>(requestOptions: new QueryRequestOptions
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
                return await _container.ReadItemAsync<ProductCategory>(id, new PartitionKey(nameof(ProductCategory)), cancellationToken: cancellationToken);
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

            var result = await _container.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<ProductCategory>>(
                StoredProcedure.UpsertProductCategory,
                new PartitionKey(nameof(ProductCategory)),
                parameters: new[] { payload },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }
    }
}
