namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CosmosService _cosmosService;

        public RefreshTokenRepository(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task<RefreshToken> GetTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cosmosService.RefreshToken.ReadItemAsync<RefreshToken>(refreshToken, new PartitionKey(refreshToken), cancellationToken: cancellationToken);
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                throw;
            }
        }

        public async Task<bool> TryDeleteTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cosmosService.RefreshToken.DeleteItemAsync<dynamic>(refreshToken, new PartitionKey(refreshToken), cancellationToken: cancellationToken);
                return true;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                throw;
            }
        }

        public async Task UpsertTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _cosmosService.RefreshToken.UpsertItemAsync(refreshToken, new PartitionKey(refreshToken.Token), cancellationToken: cancellationToken);
        }
    }
}
