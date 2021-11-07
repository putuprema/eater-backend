namespace Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly Container _container;

        public AccountRepository(CosmosService cosmosService)
        {
            _container = cosmosService.GetContainer("Account");
        }

        public async Task<Account> GetByEmailAndRoleAsync(string email, Role role = Role.CUSTOMER, CancellationToken cancellationToken = default)
        {
            using var feedIterator = _container.GetItemLinqQueryable<Account>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(role.ToString()) })
                .Where(a => a.Email == email)
                .ToFeedIterator();

            if (feedIterator.HasMoreResults)
            {
                var result = await feedIterator.ReadNextAsync(cancellationToken);
                return result.FirstOrDefault();
            }

            return null;
        }

        public async Task<Account> GetByIdAndRoleAsync(string id, Role role = Role.CUSTOMER, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _container.ReadItemAsync<Account>(id, new PartitionKey(role.ToString()), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<Account> UpsertAsync(Account account, CancellationToken cancellationToken = default)
        {
            return await _container.UpsertItemAsync(account, new PartitionKey(account.Role.ToString()), cancellationToken: cancellationToken);
        }
    }
}
