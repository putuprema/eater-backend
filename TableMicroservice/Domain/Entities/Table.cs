namespace Domain.Entities
{
    public class Table
    {
        public string ObjectType { get; } = nameof(Table);
        public bool IsNew { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int Number { get; set; }
        public bool Active { get; set; }
        public string QrStickerUrl { get; set; }
    }
}
