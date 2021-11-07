using Application.Products.Commands.BulkUpdateCategory;
using Domain.Entities;
using Infrastructure.Config;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace API.Functions.Orchestrations
{
    public class CategoryDataUpdateOrchestration
    {
        private readonly IMediator _mediator;
        private readonly DurableFunctionConfig _durableFunctionConfig;

        public CategoryDataUpdateOrchestration(IMediator mediator, IOptions<DurableFunctionConfig> durableFunctionConfig)
        {
            _mediator = mediator;
            _durableFunctionConfig = durableFunctionConfig.Value;
        }

        [FunctionName(nameof(CategoryDataUpdateOrchestration))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var productCategory = context.GetInput<ProductCategory>();

            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var command = new BulkUpdateCategoryCommand { Category = productCategory };

            var result = await context.CallActivityWithRetryAsync<BulkUpdateCategoryResult>(nameof(BulkUpdateCategoryActivity), retryOptions, command);
            while (result.Continuation != null)
            {
                log.LogInformation($"[{nameof(CategoryDataUpdateOrchestration)}]: {JsonConvert.SerializeObject(result.Continuation)}");

                command.Continuation = result.Continuation;
                result = await context.CallActivityWithRetryAsync<BulkUpdateCategoryResult>(nameof(BulkUpdateCategoryActivity), retryOptions, command);
            }

            log.LogInformation($"[{nameof(CategoryDataUpdateOrchestration)}]: Updated {result.UpdatedItems} products with category {productCategory.Id}");
        }

        [FunctionName(nameof(BulkUpdateCategoryActivity))]
        public async Task<BulkUpdateCategoryResult> BulkUpdateCategoryActivity([ActivityTrigger] BulkUpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            return await _mediator.Send(command, cancellationToken);
        }
    }
}