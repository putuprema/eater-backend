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
            services.Configure<JwtConfig>(configuration.GetSection("JWT"));

            services.AddSingleton<CosmosService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IPasswordEncoderService, PasswordEncoderService>();
            services.AddSingleton<IAccountRepository, AccountRepository>();
            services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }
    }
}
