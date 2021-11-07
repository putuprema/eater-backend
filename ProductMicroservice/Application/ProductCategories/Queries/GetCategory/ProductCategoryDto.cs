namespace Application.ProductCategories.Queries.GetCategory
{
    public class ProductCategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int SortIndex { get; set; }
    }

    public class SimpleProductCategoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
