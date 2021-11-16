using Newtonsoft.Json;

namespace Domain.Entities
{
    public class Product
    {
        public string ObjectType { get; } = nameof(Product);
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public string Description { get; set; }
        public long Price { get; set; }
        public string ImageUrl { get; set; }
        public SimpleProductCategory Category { get; set; }
        public string PreviousCategoryId { get; set; }
        public int SalesCount { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Deleted { get; set; }
        public int Ttl { get; set; } = -1;
        [JsonProperty("_etag")]
        public string ETag { get; set; }

        public void MarkForDeletion()
        {
            Deleted = true;
            Ttl = 10;
        }
    }
}
