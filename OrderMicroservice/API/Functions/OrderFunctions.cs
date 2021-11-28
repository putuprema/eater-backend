using Application.Orders.Commands.CreateOrder;
using Application.Orders.Queries.GetActiveOrder;
using Application.Orders.Queries.GetActiveOrders;
using Application.Orders.Queries.GetCustomerOrder;
using Application.Orders.Queries.GetOrdersByCustomer;
using Domain.Entities;
using Eater.Shared.Constants;
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

        [FunctionName(nameof(GetActiveOrdersById))]
        public async Task<IActionResult> GetActiveOrdersById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/active-order/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var order = await _mediator.Send(new GetActiveOrderQuery { OrderId = id }, cancellationTokens);
            if (order == null)
                return new NotFoundResult();

            return new OkObjectResult(order);
        }

        [FunctionName(nameof(GetActiveOrders))]
        public async Task<IActionResult> GetActiveOrders([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/active-order")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            var orders = await _mediator.Send(new GetActiveOrdersQuery(), cancellationTokens);
            return new OkObjectResult(orders);
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

                    await durableClient.RaiseEventAsync(id, Events.OrderStatus.OrderStatusChanged, orderStatus);
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
        public async Task<IActionResult> CreateOrder([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<CreateOrderCommand>();
                if (request == null)
                    return new BadRequestResult();

                var userClaims = req.GetDefaultUserClaims();

                request.Customer = new CustomerDetails
                {
                    Id = userClaims.Id,
                    Name = userClaims.Name,
                    Email = userClaims.Email
                };

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

        [FunctionName(nameof(GetCustomerOrders))]
        public async Task<IActionResult> GetCustomerOrders([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/order/search")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var query = await req.ReadFromJsonAsync<GetOrdersByCustomerQuery>();
            if (query == null)
                return new BadRequestResult();

            try
            {
                var userClaims = req.GetDefaultUserClaims();

                query.CustomerId = userClaims.Id;

                var orders = await _mediator.Send(query, cancellationTokens);
                return new OkObjectResult(orders);
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }

        [FunctionName(nameof(GetCustomerOrderById))]
        public async Task<IActionResult> GetCustomerOrderById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/order/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var userClaims = req.GetDefaultUserClaims();

                var order = await _mediator.Send(new GetCustomerOrderQuery { CustomerId = userClaims.Id, OrderId = id }, cancellationTokens);
                if (order == null)
                    return new NotFoundResult();

                return new OkObjectResult(order);
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
