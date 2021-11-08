namespace Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest
    {
        public string Id { get; set; }
    }
}
