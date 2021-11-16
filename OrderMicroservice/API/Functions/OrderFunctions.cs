using Application.Orders.Commands.CreateOrder;
using Domain.Entities;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace API.Functions
{
    public class OrderFunctions
    {
        private readonly IMediator _mediator;

        public OrderFunctions(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(UpdateOrderStatus))]
        public async Task<IActionResult> UpdateOrderStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/order/{id}/status")] HttpRequest req, string id,
            [DurableClient] IDurableOrchestrationClient durableClient)
        {
            try
            {
                if (Enum.TryParse<OrderStatus>(req.Query["value"], out var orderStatus))
                {
                    var orderOrchestration = await durableClient.GetStatusAsync(id);
                    if (orderOrchestration == null)
                    {
                        throw new NotFoundException("Order not found or is not active");
                    }
                    if (orderOrchestration.RuntimeStatus == OrchestrationRuntimeStatus.Completed
                        || orderOrchestration.RuntimeStatus == OrchestrationRuntimeStatus.Terminated
                        || orderOrchestration.RuntimeStatus == OrchestrationRuntimeStatus.Canceled)
                    {
                        throw new BadRequestException("This order is no longer active");
                    }

                    await durableClient.RaiseEventAsync(id, OrderEvents.OrderStatusChanged, orderStatus);
                    return new OkResult();
                }

                return new BadRequestResult();
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }

        [FunctionName(nameof(CreateOrder))]
        public async Task<IActionResult> CreateOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<CreateOrderCommand>();
                if (request == null)
                    return new BadRequestResult();

                request.Customer = new CustomerDetails
                {
                    Id = req.GetCurrentUserId(),
                    Name = req.GetCurrentUserName(),
                    Email = req.GetCurrentUserEmail()
                };

                var result = await _mediator.Send(request, cancellationToken);
                await starter.StartNewAsync(Orchestrations.OrderOrchestration, result.Id, result);

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
