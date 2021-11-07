using Application.Accounts.Commands.Auth;

namespace API.Functions
{
    public class AuthFunctions
    {
        private readonly IMediator _mediator;

        public AuthFunctions(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(AuthToken))]
        public async Task<IActionResult> AuthToken([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/auth/token")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var request = await req.ReadFromJsonAsync<AuthCommand>();
                if (request == null)
                    return new BadRequestResult();

                var result = await _mediator.Send(request, cancellationTokens);
                if (result == null)
                    return new UnauthorizedResult();

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
