using Application.Payments.Commands.PaymentNotification;
using Infrastructure.Config;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Options;

namespace API.Functions.Orchestrations
{
    public class PaymentNotificationOrchestration
    {
        private readonly IMediator _mediator;
        private readonly DurableFunctionConfig _durableFunctionConfig;

        public PaymentNotificationOrchestration(IMediator mediator, IOptions<DurableFunctionConfig> durableFunctionConfig)
        {
            _mediator = mediator;
            _durableFunctionConfig = durableFunctionConfig.Value;
        }

        [FunctionName(nameof(PaymentNotificationOrchestration))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var retryOptions = new RetryOptions(
                firstRetryInterval: TimeSpan.FromSeconds(_durableFunctionConfig.FirstRetryIntervalSecond),
                maxNumberOfAttempts: _durableFunctionConfig.MaxNumberOfAttempts);

            var payload = context.GetInput<PaymentNotificationOrchestrationModel>();
            await context.CallActivityWithRetryAsync<Application.Payments.Query.GetPaymentInfo.PaymentDto>(nameof(HandlePaymentNotificationActivity), retryOptions, payload);
        }

        [FunctionName(nameof(HandlePaymentNotificationActivity))]
        public async Task HandlePaymentNotificationActivity([ActivityTrigger] PaymentNotificationOrchestrationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new PaymentNotificationCommand
            {
                Payload = model.Payload,
                Signature = model.Signature
            },
            cancellationToken);
        }
    }
}