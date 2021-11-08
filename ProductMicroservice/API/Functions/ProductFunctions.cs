using Application.Products.Commands.AddProduct;
using Application.Products.Commands.DeleteProduct;
using Application.Products.Commands.EditProduct;
using Application.Products.Queries.GetFeaturedProducts;
using Application.Products.Queries.GetProduct;
using Application.Products.Queries.GetProducts;

namespace API.Functions
{
    public class ProductFunctions
    {
        private readonly ILogger<ProductFunctions> _logger;
        private readonly IMediator _mediator;

        public ProductFunctions(ILogger<ProductFunctions> log, IMediator mediator)
        {
            _logger = log;
            _mediator = mediator;
        }

        [FunctionName(nameof(GetFeaturedProducts))]
        public async Task<IActionResult> GetFeaturedProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/featured")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var result = await _mediator.Send(new GetFeaturedProductsQuery(), cancellationTokens);
            return new OkObjectResult(result);
        }

        [FunctionName(nameof(GetProductById))]
        public async Task<IActionResult> GetProductById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var product = await _mediator.Send(new GetProductQuery { Id = id }, cancellationTokens);
            if (product == null)
                return new NotFoundResult();

            return new OkObjectResult(product);
        }

        [FunctionName(nameof(EditProduct))]
        public async Task<IActionResult> EditProduct(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var request = await req.ReadFromJsonAsync<EditProductCommand>();
            if (request == null)
                return new BadRequestResult();

            try
            {
                request.Id = id;
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

        [FunctionName(nameof(DeleteProduct))]
        public async Task<IActionResult> DeleteProduct(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id}")] HttpRequest req, string id, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            try
            {
                await _mediator.Send(new DeleteProductCommand { Id = id }, cancellationTokens);
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

        [FunctionName(nameof(GetProducts))]
        public async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products/search")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;

            var query = await req.ReadFromJsonAsync<GetProductsQuery>();
            if (query == null)
                return new BadRequestResult();

            try
            {
                var result = await _mediator.Send(query, cancellationTokens);
                return new OkObjectResult(result);
            }
            catch (AppException ex)
            {
                return new ObjectResult(ex.GetResponse())
                {
                    StatusCode = ex.StatusCode
                };
            }
        }

        [FunctionName(nameof(AddProduct))]
        public async Task<IActionResult> AddProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequest req, CancellationToken cancellationToken)
        {
            var cancellationTokens = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, req.HttpContext.RequestAborted).Token;
            var formData = await req.ReadFormAsync(cancellationTokens);

            try
            {
                var request = new AddProductCommand
                {
                    Name = formData["name"],
                    Description = formData["description"],
                    CategoryId = formData["categoryId"],
                    Image = req.Form.Files["image"]
                };

                if (long.TryParse(formData["price"], out var price))
                {
                    request.Price = price;
                }

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
