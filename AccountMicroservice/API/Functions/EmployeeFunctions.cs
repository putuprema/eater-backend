using Application.Accounts.Commands.AddEmployee;

namespace API.Functions
{
    public class EmployeeFunctions
    {
        private readonly IMediator _mediator;

        public EmployeeFunctions(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(AddEmployee))]
        public async Task<IActionResult> AddEmployee([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/employee")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<AddEmployeeCommand>();
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
