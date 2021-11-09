using Application.Common.Interfaces;
using Application.Products.Commands.BulkUpdateCategory;
using Domain.Entities;
using Infrastructure.Config;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Functions.Orchestrations
{
    public class ProductCategoryUpdateOrchestration
    {
        private readonly IMediator _mediator;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly DurableFunctionConfig _durableFunctionConfig;

        public ProductCategoryUpdateOrchestration(IMediator mediator, IOptions<DurableFunctionConfig> durableFunctionConfig, IProductCategoryRepository productCategoryRepository)
        {
            _mediator = mediator;
            _durableFunctionConfig = durableFunctionConfig.Value;
            _productCategoryRepository = productCategoryRepository;
        }

        [FunctionName(nameof(ProductCategoryUpdateOrchestration))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var productCategory = context.GetInput<ProductCategory>();

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            await context.CallActivityWithRetryAsync(nameof(PopulateProductCategoryCache), retryOptions, null);
            await context.CallActivityWithRetryAsync(nameof(BulkUpdateCategoryActivity), retryOptions, productCategory);
        }

        [FunctionName(nameof(PopulateProductCategoryCache))]
        public async Task PopulateProductCategoryCache([ActivityTrigger] IDurableActivityContext context, ILogger log, CancellationToken cancellationToken)
        {
            var count = await _productCategoryRepository.PopulateCategoryCacheAsync(cancellationToken);
            log.LogInformation($"[{nameof(PopulateProductCategoryCache)}]: Populated {count} product categories to cache");
        }

        [FunctionName(nameof(BulkUpdateCategoryActivity))]
        public async Task BulkUpdateCategoryActivity([ActivityTrigger] ProductCategory productCategory, ILogger log, CancellationToken cancellationToken)
        {
            var command = new BulkUpdateCategoryCommand { Category = productCategory };

            var result = await _mediator.Send(command, cancellationToken);
            while (result.Continuation != null)
            {
                log.LogInformation($"[{nameof(BulkUpdateCategoryActivity)}]: {JsonConvert.SerializeObject(result.Continuation)}");

                command.Continuation = result.Continuation;
                result = await _mediator.Send(command, cancellationToken);
            }

            log.LogInformation($"[{nameof(BulkUpdateCategoryActivity)}]: Updated {result.UpdatedItems} products with category {productCategory.Id}");
        }
    }
}