using Application.Accounts.Commands.RegisterCustomer;

namespace API.Functions
{
    public class CustomerFunctions
    {
        private readonly IMediator _mediator;

        public CustomerFunctions(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(RegisterCustomer))]
        public async Task<IActionResult> RegisterCustomer([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/customer")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<RegisterCustomerCommand>();
                if (request == null)
                    return new BadRequestResult();

                await _mediator.Send(request, cancellationTokens);
                return new OkResult();
            }
            catch (ValidationException ex)
            {
                return ex.GetResponse();
            }
        }
    }
}
