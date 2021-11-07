using Application.ProductCategories.Queries.GetCategory;
using Application.Products.Queries.GetProducts;

namespace Application.Products.Queries.GetFeaturedProducts
{
    public class FeaturedProductsDto
    {
        public ProductCategoryDto Category { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
    }
}
