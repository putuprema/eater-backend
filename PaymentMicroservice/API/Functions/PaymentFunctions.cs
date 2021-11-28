using API.Functions.Orchestrations;
using Application.Payments.Commands.InitPayment;
using Application.Payments.Query.GetPaymentInfo;
using Eater.Shared.Common;
using Eater.Shared.Constants;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace API.Functions
{
    public class PaymentFunctions
    {
        private readonly IMediator _mediator;

        public PaymentFunctions(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(PaymentNotificationWebhook))]
        public async Task<IActionResult> PaymentNotificationWebhook(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/payment/webhook")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            CancellationToken cancellationToken)
        {
            await starter.StartNewAsync(nameof(PaymentNotificationOrchestration), new PaymentNotificationOrchestrationModel
            {
                Payload = await req.ReadAsStringAsync(),
                Signature = req.Headers["Stripe-Signature"]
            });

            return new OkResult();
        }

        [FunctionName(nameof(InitPayment))]
        [return: ServiceBus(QueueNames.OrderOrchestrationEvent, Connection = AppSettingsKeys.ServiceBusConnString)]
        public async Task<EventEnvelope<PaymentDto>> InitPayment(
            [ServiceBusTrigger(QueueNames.InitPaymentCmd, Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            CancellationToken cancellationToken)
        {
            var command = JsonConvert.DeserializeObject<InitPaymentCommand>(myQueueItem);
            var resultingEvent = new EventEnvelope<PaymentDto>(command.OrderId, nameof(Events.InitPayment), Events.InitPayment.InitPaymentSuccess, null);

            try
            {
                var result = await _mediator.Send(command, cancellationToken);
                resultingEvent.Body = result;
            }
            catch (AppException)
            {
                resultingEvent.EventType = Events.InitPayment.InitPaymentFailed;
            }

            return resultingEvent;
        }
    }
}
