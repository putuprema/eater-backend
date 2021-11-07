namespace Application.Queries.GetAllTables
{
    public class TableFullDto
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public bool Active { get; set; }
        public string QrStickerUrl { get; set; }
    }
}
