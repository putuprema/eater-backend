using Application.Accounts.Queries.GetIdentity;

namespace API.Functions
{
    public class Identity
    {
        private readonly IMediator _mediator;

        public Identity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(Identity))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/me")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            try
            {
                var query = new GetIdentityQuery
                {
                    UserId = req.GetCurrentUserId(),
                    Role = req.GetCurrentUserRole()
                };

                var result = await _mediator.Send(query, cancellationTokens);
                if (result == null)
                    return new UnauthorizedResult();

                return new OkObjectResult(result);
            }
            catch (ValidationException ex)
            {
                return ex.GetResponse();
            }
        }
    }
}
