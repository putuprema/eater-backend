using Infrastructure.Config;
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

            services.AddSingleton<CosmosService>();
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<IActiveOrderRepository, ActiveOrderRepository>();

            return services;
        }
    }
}
