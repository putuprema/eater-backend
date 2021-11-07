using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class CosmosService
    {
        private readonly string databaseId;
        public readonly CosmosClient Client;

        public CosmosService(IConfiguration config)
        {
            databaseId = config.GetValue<string>("CosmosDbName");

            var connString = config.GetValue<string>("CosmosDbConnString");
            Client = new CosmosClient(connString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        }

        public Container Account
        {
            get => GetContainer(nameof(Account));
        }

        public Container RefreshToken
        {
            get => GetContainer(nameof(RefreshToken));
        }

        public Container GetContainer(string containerId) => Client.GetContainer(databaseId, containerId);
    }
}
