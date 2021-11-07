using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DurableFunctionConfig>(configuration.GetSection("DurableFunctionConfig"));
            services.Configure<BlobStorageConfig>(configuration.GetSection("BlobStorage"));
            services.Configure<CosmosConfig>(configuration.GetSection("CosmosConfig"));

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IOptions<BlobStorageConfig>>().Value;
                return new BlobServiceClient(config.ConnString);
            });

            services.AddSingleton<CosmosService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<IQrCodeService, QrCodeService>();
            services.AddScoped<ITableRepository, TableRepository>();

            return services;
        }
    }
}
