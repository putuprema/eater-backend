using Application.Commands.GenerateTable;
using Application.Commands.SetActive;

namespace API.Functions
{
    public class TableCommands
    {
        private readonly IMediator _mediator;

        public TableCommands(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GenerateTable))]
        public async Task<IActionResult> GenerateTable([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/table")] HttpRequest req)
        {
            try
            {
                var request = await req.ReadFromJsonAsync<GenerateTableCommand>();
                if (request == null)
                    return new BadRequestResult();

                await _mediator.Send(request, req.HttpContext.RequestAborted);
                return new OkResult();
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

        [FunctionName(nameof(SetActive))]
        public async Task<IActionResult> SetActive([HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/table/{id}")] HttpRequest req, string id)
        {
            try
            {
                var request = await req.ReadFromJsonAsync<SetActiveCommand>();
                if (request == null)
                    return new BadRequestResult();

                request.TableId = id;

                var result = await _mediator.Send(request, req.HttpContext.RequestAborted);
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
