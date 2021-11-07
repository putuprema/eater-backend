namespace Infrastructure.Config
{
    public class BlobStorageConfig
    {
        public string ConnString { get; set; }
        public int MaxFileSizeMb { get; set; }
    }
}
