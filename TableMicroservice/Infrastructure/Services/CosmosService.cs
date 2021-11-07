namespace Infrastructure.Services
{
    public class CosmosService
    {
        public readonly CosmosConfig Config;
        public readonly CosmosClient Client;

        public CosmosService(IOptions<CosmosConfig> config)
        {
            Config = config.Value;

            Client = new CosmosClient(Config.ConnString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
        }

        public Container Items
        {
            get => GetContainer(nameof(Items));
        }

        public Container GetContainer(string containerId) => Client.GetContainer(Config.DbName, containerId);
    }
}
