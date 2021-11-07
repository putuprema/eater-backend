namespace Domain.Entities
{
    public class ProductCategory
    {
        public string ObjectType { get; } = nameof(ProductCategory);
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public int SortIndex { get; set; }
    }

    public class SimpleProductCategory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
    }
}
