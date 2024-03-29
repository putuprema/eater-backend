﻿using Azure.Storage.Blobs;
using Infrastructure.Config;
using Infrastructure.Repositories;
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
            services.AddSingleton<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<IFeaturedProductsService, FeaturedProductsService>();

            return services;
        }
    }
}
