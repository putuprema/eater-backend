using Azure.Storage.Blobs;
using Infrastructure.Config;
using Infrastructure.Integrations.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DurableFunctionConfig>(configuration.GetSection("DurableFunctionConfig"));
            services.Configure<CosmosConfig>(configuration.GetSection("CosmosConfig"));

            services.AddSingleton(provider =>
            {
                var connString = configuration["AzureWebJobsStorage"];
                return new BlobServiceClient(connString);
            });

            services.AddSingleton<CosmosService>();
            services.AddSingleton<IPaymentRepository, PaymentRepository>();
            services.AddSingleton<IPaymentIntegrationService, StripeIntegrationService>();

            return services;
        }
    }
}
