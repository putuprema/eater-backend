using Application.Common.Interfaces;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.Orchestrations
{
    public class TableGenerationOrchestration
    {
        private readonly DurableFunctionConfig _durableFunctionConfig;
        private readonly ITableRepository _tableRepository;
        private readonly IQrCodeService _qrCodeService;

        public TableGenerationOrchestration(IOptions<DurableFunctionConfig> durableFunctionConfig, ITableRepository tableRepository, IQrCodeService qrCodeService)
        {
            _durableFunctionConfig = durableFunctionConfig.Value;
            _tableRepository = tableRepository;
            _qrCodeService = qrCodeService;
        }

        [FunctionName(nameof(TableGenerationOrchestration))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var table = context.GetInput<Table>();
            table.QrStickerUrl = await context.CallActivityWithRetryAsync<string>(nameof(GenerateQrCodeSticker), retryOptions, table);

            await context.CallActivityWithRetryAsync(nameof(FinalizeTableGeneration), retryOptions, table);
        }

        [FunctionName(nameof(GenerateQrCodeSticker))]
        public async Task<string> GenerateQrCodeSticker([ActivityTrigger] Table table, ILogger log, CancellationToken cancellationToken)
        {
            var qrStickerUrl = await _qrCodeService.GenerateQrStickerAsync(table, cancellationToken);
            log.LogInformation($"Generated QR code sticker for table {table.Id}");
            return qrStickerUrl;
        }

        [FunctionName(nameof(FinalizeTableGeneration))]
        public async Task FinalizeTableGeneration([ActivityTrigger] Table table, CancellationToken cancellationToken)
        {
            table.IsNew = false;
            await _tableRepository.UpsertAsync(table, cancellationToken);
        }
    }
}