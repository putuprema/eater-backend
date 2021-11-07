namespace Application.ProductCategories.Queries.GetCategory
{
    public class GetCategoryQuery : IRequest<ProductCategoryDto>
    {
        public string Id { get; set; }
    }
}
