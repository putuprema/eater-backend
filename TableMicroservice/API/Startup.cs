﻿using Application;
using Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: FunctionsStartup(typeof(API.Startup))]
namespace API
{
    public class Startup : FunctionsStartup
    {
        private string ApplicationRootPath { get; set; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            ApplicationRootPath = builder.GetContext().ApplicationRootPath;

            IConfiguration configuration = builder.GetContext().Configuration;
            ConfigureServices(builder.Services, configuration);
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication();
            services.AddInfrastructure(configuration);

            services.Configure<AppConfig>(options =>
            {
                options.ApplicationRootPath = ApplicationRootPath;
            });

            services.AddHttpContextAccessor();
            services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.Converters.Add(new JsonStringEnumConverter());
            });
            services.Configure<JsonSerializerSettings>(options =>
            {
                options.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                options.Converters.Add(new StringEnumConverter());
            });
        }
    }
}
