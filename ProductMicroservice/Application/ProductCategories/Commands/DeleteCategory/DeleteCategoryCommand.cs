namespace Application.ProductCategories.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest
    {
        public string Id { get; set; }
    }
}
