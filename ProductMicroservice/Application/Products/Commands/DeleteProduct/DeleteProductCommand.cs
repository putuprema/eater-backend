namespace Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<Product>
    {
        public string Id { get; set; }
    }
}
