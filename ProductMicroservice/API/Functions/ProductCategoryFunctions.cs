using Application.ProductCategories.Commands.DeleteCategory;
using Application.ProductCategories.Commands.UpsertCategory;
using Application.ProductCategories.Queries.GetCategories;
using Application.ProductCategories.Queries.GetCategory;

namespace API.Functions
{
    public class ProductCategoryFunctions
    {
        private readonly ILogger<ProductCategoryFunctions> _logger;
        private readonly IMediator _mediator;

        public ProductCategoryFunctions(ILogger<ProductCategoryFunctions> log, IMediator mediator)
        {
            _logger = log;
            _mediator = mediator;
        }

        [FunctionName(nameof(GetProductCategories))]
        public async Task<IActionResult> GetProductCategories(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/category")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            var result = await _mediator.Send(new GetCategoriesQuery(), cancellationTokens);
            return new OkObjectResult(result);
        }

        [FunctionName(nameof(GetProductCategoryById))]
        public async Task<IActionResult> GetProductCategoryById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/category/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var result = await _mediator.Send(new GetCategoryQuery { Id = id }, cancellationTokens);
            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }

        [FunctionName(nameof(DeleteProductCategory))]
        public async Task<IActionResult> DeleteProductCategory(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/category/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            try
            {
                await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationTokens);
                return new OkResult();
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }

        [FunctionName(nameof(UpsertProductCategory))]
        public async Task<IActionResult> UpsertProductCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "v1/category")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var request = await req.ReadFromJsonAsync<UpsertCategoryCommand>();
            if (request == null)
                return new BadRequestResult();

            try
            {
                await _mediator.Send(request, cancellationTokens);
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
    }
}
