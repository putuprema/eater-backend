using API.Functions.Orchestrations;
using Application.Payments.Commands.InitPayment;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

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
        public async Task<IActionResult> InitPayment([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/payment/init")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<InitPaymentCommand>();
                if (request == null)
                    return new BadRequestResult();

                var result = await _mediator.Send(request, cancellationTokens);
                return new OkObjectResult(result);
            }
            catch (ValidationException ex)
            {
                return ex.GetResponse();
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }
    }
}
