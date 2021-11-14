namespace API.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IActionResult GetResponse(this ValidationException ex)
        {
            return new BadRequestObjectResult(new
            {
                Message = "Bad Request",
                ValidationErrors = ex.Errors
            });
        }
    }
}
