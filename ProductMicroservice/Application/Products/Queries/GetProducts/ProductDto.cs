using Application.ProductCategories.Queries.GetCategory;

namespace Application.Products.Queries.GetProducts
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }
        public string ImageUrl { get; set; }
        public SimpleProductCategoryDto Category { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
