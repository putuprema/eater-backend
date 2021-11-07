using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Config;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.Orchestrations
{
    public class ProductCleanupOrchestration
    {
        private readonly DurableFunctionConfig _durableFunctionConfig;
        private readonly IStorageService _storageService;

        public ProductCleanupOrchestration(IOptions<DurableFunctionConfig> durableFunctionConfig, IStorageService storageService)
        {
            _durableFunctionConfig = durableFunctionConfig.Value;
            _storageService = storageService;
        }

        [FunctionName(nameof(ProductCleanupOrchestration))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var product = context.GetInput<Product>();

            // Delete product image
            await context.CallActivityWithRetryAsync(nameof(DeleteProductImageActivity), retryOptions, product.ImageUrl);
        }

        [FunctionName(nameof(DeleteProductImageActivity))]
        public async Task DeleteProductImageActivity([ActivityTrigger] string imageUrl, ILogger log)
        {
            await _storageService.DeleteAsync(imageUrl);
            log.LogInformation($"[{nameof(DeleteProductImageActivity)}]: Deleted blob {imageUrl}");
        }
    }
}