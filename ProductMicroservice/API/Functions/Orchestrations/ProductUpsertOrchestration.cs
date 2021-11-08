using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Config;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.Orchestrations
{
    public class ProductUpsertOrchestration
    {
        private readonly DurableFunctionConfig _durableFunctionConfig;
        private readonly IProductRepository _productRepository;

        public ProductUpsertOrchestration(IOptions<DurableFunctionConfig> durableFunctionConfig, IProductRepository productRepository)
        {
            _durableFunctionConfig = durableFunctionConfig.Value;
            _productRepository = productRepository;
        }

        [FunctionName(nameof(ProductUpsertOrchestration))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var product = context.GetInput<Product>();

            if (product.PreviousCategoryId != product.Category.Id)
            {
                await context.CallActivityWithRetryAsync(nameof(DeleteProductOnPrevCategoryPartition), retryOptions, product);
                await context.CallActivityWithRetryAsync(nameof(UpdatePreviousCategoryId), retryOptions, product);
            }
            else
            {
                await context.CallActivityWithRetryAsync(nameof(ReplicateProductUpsert), retryOptions, product);
            }
        }

        [FunctionName(nameof(DeleteProductOnPrevCategoryPartition))]
        public async Task DeleteProductOnPrevCategoryPartition([ActivityTrigger] Product product, CancellationToken cancellationToken)
        {
            try
            {
                await _productRepository.DeleteProductByCategoryAsync(product.Id, product.PreviousCategoryId, cancellationToken);
            }
            catch (NotFoundException)
            {
            }
        }

        [FunctionName(nameof(UpdatePreviousCategoryId))]
        public async Task UpdatePreviousCategoryId([ActivityTrigger] Product product, CancellationToken cancellationToken)
        {
            product.PreviousCategoryId = product.Category.Id;
            await _productRepository.UpsertAsync(product, cancellationToken);
        }

        [FunctionName(nameof(ReplicateProductUpsert))]
        public async Task ReplicateProductUpsert([ActivityTrigger] Product product, CancellationToken cancellationToken)
        {
            await _productRepository.UpsertProductByCategoryAsync(product, cancellationToken);
        }
    }
}