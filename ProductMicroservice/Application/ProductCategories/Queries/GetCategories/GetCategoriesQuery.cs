using Application.ProductCategories.Queries.GetCategory;

namespace Application.ProductCategories.Queries.GetCategories
{
    public class GetCategoriesQuery : IRequest<IEnumerable<ProductCategoryDto>>
    {
    }
}
