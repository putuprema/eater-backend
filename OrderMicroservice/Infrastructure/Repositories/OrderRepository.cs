using Application.Orders.Queries.GetOrdersByCustomer;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CosmosService _cosmosService;

        public OrderRepository(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task<PagedResultSet<Order>> GetByCustomerAsync(GetOrdersByCustomerQuery query, CancellationToken cancellationToken = default)
        {
            var feedIterator = _cosmosService.Orders.GetItemLinqQueryable<Order>(continuationToken: query.ContinuationToken, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(query.CustomerId),
                MaxItemCount = query.PageSize
            })
                .OrderBy(x => x.CreatedOn)
                .ToFeedIterator();

            string continuationToken = null;
            var orders = new List<Order>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                orders.AddRange(response);

                if (response.Count > 0)
                {
                    continuationToken = response.ContinuationToken;
                    break;
                }
            }

            return new PagedResultSet<Order>
            {
                ContinuationToken = continuationToken,
                Items = orders
            };
        }

        public async Task<Order> GetByOrderIdAndCustomerIdAsync(string orderId, string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.Orders.ReadItemAsync<Order>(orderId, new PartitionKey(customerId), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw ex;
            }
        }

        public async Task<Order> UpsertAsync(Order order, CancellationToken cancellationToken = default)
        {
            order.UpdatedOn = DateTime.UtcNow;
            return await _cosmosService.Orders.UpsertItemAsync(order,
                new PartitionKey(order.Customer.Id),
                new ItemRequestOptions { IfMatchEtag = order.ETag },
                cancellationToken: cancellationToken);
        }
    }
}
