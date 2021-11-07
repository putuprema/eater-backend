using Application.Products.Commands.BulkUpdateCategory;
using Application.Products.Queries.GetProducts;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly Container _container;
        private readonly IOptions<JsonSerializerSettings> _jsonSerializerSettings;

        public ProductRepository(CosmosService cosmosService, IOptions<JsonSerializerSettings> jsonSerializerSettings)
        {
            _container = cosmosService.GetContainer("Items");
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        private async Task<PagedResultSet<Product>> DoPagedQueryAsync(FeedIterator<Product> feedIterator, CancellationToken cancellationToken)
        {
            string continuationToken = null;
            var products = new List<Product>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                products.AddRange(response);

                if (response.Count > 0)
                {
                    continuationToken = response.ContinuationToken;
                    break;
                }
            }

            return new PagedResultSet<Product>
            {
                ContinuationToken = continuationToken,
                Items = products
            };
        }

        public async Task<PagedResultSet<Product>> FindAllAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            IQueryable<Product> queryBuilder = _container.GetItemLinqQueryable<Product>(continuationToken: query.ContinuationToken, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(nameof(Product)),
                MaxItemCount = query.PageSize
            });

            if (!string.IsNullOrEmpty(query.Name))
            {
                queryBuilder = queryBuilder.Where(p => p.Name.Contains(query.Name));
            }

            if (!string.IsNullOrEmpty(query.CategoryId))
            {
                queryBuilder = queryBuilder.Where(p => p.Category.Id == query.CategoryId);
            }

            using var feedIterator = queryBuilder.OrderBy(p => p.Name).ToFeedIterator();
            return await DoPagedQueryAsync(feedIterator, cancellationToken);
        }

        public async Task<Product> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _container.ReadItemAsync<Product>(id, new PartitionKey(nameof(Product)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<Product> UpsertAsync(Product product, CancellationToken cancellationToken = default)
        {
            return await _container.UpsertItemAsync(product, new PartitionKey(nameof(Product)), cancellationToken: cancellationToken);
        }

        public async Task<BulkUpdateCategoryResult> BulkUpdateCategoryDataAsync(ProductCategory category, BulkUpdateCategoryContinuation continuation, CancellationToken cancellationToken = default)
        {
            var categoryPayload = JsonConvert.SerializeObject(category, _jsonSerializerSettings.Value);
            var continuationPayload = JsonConvert.SerializeObject(continuation, _jsonSerializerSettings.Value);

            var result = await _container.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<BulkUpdateCategoryResult>>(
                StoredProcedure.UpdateCategoryDataProduct,
                new PartitionKey(nameof(Product)),
                parameters: new[] { categoryPayload, continuationPayload },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }

        public async Task<Product> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _container.DeleteItemAsync<Product>(id, new PartitionKey(nameof(Product)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Product not found");

                throw ex;
            }
        }

        public async Task<IDictionary<string, IEnumerable<Product>>> GetFeaturedProductsPerCategoryAsync(IEnumerable<string> categoryIds, CancellationToken cancellationToken = default)
        {
            var result = await _container.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<IDictionary<string, IEnumerable<Product>>>>(
                StoredProcedure.GetFeaturedProductsPerCategory,
                new PartitionKey(nameof(Product)),
                parameters: new[] { JsonConvert.SerializeObject(categoryIds) },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }
    }
}
