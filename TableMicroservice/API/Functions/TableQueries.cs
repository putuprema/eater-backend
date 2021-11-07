using Application.Queries.GetAllTables;
using Application.Queries.GetTableById;

namespace API.Functions
{
    public class TableQueries
    {
        private readonly IMediator _mediator;

        public TableQueries(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetAllTables))]
        public async Task<IActionResult> GetAllTables([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/table")] HttpRequest req)
        {
            var result = await _mediator.Send(new GetAllTablesQuery(), req.HttpContext.RequestAborted);
            return new OkObjectResult(result);
        }

        [FunctionName(nameof(GetTableById))]
        public async Task<IActionResult> GetTableById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/table/{id}")] HttpRequest req, string id)
        {
            var result = await _mediator.Send(new GetTableByIdQuery { TableId = id }, req.HttpContext.RequestAborted);

            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }
    }
}
