using Application.Products.Commands.BulkUpdateCategory;
using Application.Products.Queries.GetProducts;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CosmosService _cosmosService;
        private readonly IOptions<JsonSerializerSettings> _jsonSerializerSettings;

        public ProductRepository(CosmosService cosmosService, IOptions<JsonSerializerSettings> jsonSerializerSettings)
        {
            _cosmosService = cosmosService;
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

        public async Task<BulkUpdateCategoryResult> BulkUpdateCategoryDataAsync(ProductCategory category, BulkUpdateCategoryContinuation continuation, CancellationToken cancellationToken = default)
        {
            var categoryPayload = JsonConvert.SerializeObject(category, _jsonSerializerSettings.Value);
            var continuationPayload = JsonConvert.SerializeObject(continuation, _jsonSerializerSettings.Value);

            var result = await _cosmosService.Items.Scripts.ExecuteStoredProcedureAsync<CosmosSpResult<BulkUpdateCategoryResult>>(
                StoredProcedure.UpdateCategoryDataProduct,
                new PartitionKey(nameof(Product)),
                parameters: new[] { categoryPayload, continuationPayload },
                cancellationToken: cancellationToken);

            result.Resource.EnsureSuccessStatusCode();
            return result.Resource.Data;
        }

        public async Task<PagedResultSet<Product>> FindAllAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(query.CategoryId))
            {
                return await GetByCategoryIdAsync(query, cancellationToken);
            }

            IQueryable<Product> queryBuilder = _cosmosService.Items.GetItemLinqQueryable<Product>(continuationToken: query.ContinuationToken, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(nameof(Product)),
                MaxItemCount = query.PageSize
            });

            if (!string.IsNullOrEmpty(query.Name))
            {
                queryBuilder = queryBuilder.Where(p => p.Name.Contains(query.Name));
            }

            using var feedIterator = queryBuilder.Where(p => !p.Deleted).OrderBy(p => p.Name).ToFeedIterator();
            return await DoPagedQueryAsync(feedIterator, cancellationToken);
        }

        public async Task<PagedResultSet<Product>> GetByCategoryIdAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
        {
            IQueryable<Product> queryBuilder = _cosmosService.ProductByCategory.GetItemLinqQueryable<Product>(continuationToken: query.ContinuationToken, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(query.CategoryId),
                MaxItemCount = query.PageSize
            });

            if (!string.IsNullOrEmpty(query.Name))
            {
                queryBuilder = queryBuilder.Where(p => p.Name.Contains(query.Name));
            }

            using var feedIterator = queryBuilder.Where(p => !p.Deleted).OrderBy(p => p.Name).ToFeedIterator();
            return await DoPagedQueryAsync(feedIterator, cancellationToken);
        }

        public async Task<Product> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _cosmosService.Items.ReadItemAsync<Product>(id, new PartitionKey(nameof(Product)), cancellationToken: cancellationToken);
                if (product.Resource.Deleted)
                    return null;

                return product;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<Product> UpsertAsync(Product product, bool denyOnVersionMismatch = false, CancellationToken cancellationToken = default)
        {
            return await _cosmosService.Items.UpsertItemAsync(product, 
                new PartitionKey(nameof(Product)),
                new ItemRequestOptions { IfMatchEtag = denyOnVersionMismatch ? product.ETag : null },
                cancellationToken);
        }

        public async Task<Product> UpsertProductByCategoryAsync(Product product, CancellationToken cancellationToken = default)
        {
            return await _cosmosService.ProductByCategory.UpsertItemAsync(product, new PartitionKey(product.Category.Id), cancellationToken: cancellationToken);
        }

        public async Task<Product> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Items.DeleteItemAsync<Product>(id, new PartitionKey(nameof(Product)), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Product not found");

                throw ex;
            }
        }

        public async Task<Product> DeleteProductByCategoryAsync(string id, string categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.ProductByCategory.DeleteItemAsync<Product>(id, new PartitionKey(categoryId), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new NotFoundException("Product not found");

                throw ex;
            }
        }

        public async Task<IEnumerable<Product>> GetAllByIdsAsync(List<string> ids, CancellationToken cancellationToken = default)
        {
            var feedIterator = _cosmosService.Items.GetItemLinqQueryable<Product>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(nameof(Product)) })
                .Where(p => ids.Contains(p.Id))
                .ToFeedIterator();

            var products = new List<Product>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                products.AddRange(response);
            }

            return products;
        }
    }
}
