namespace Infrastructure.Cosmos
{
    public class CosmosSpResult<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public void EnsureSuccessStatusCode()
        {
            if (Status != 200)
            {
                throw new AppException(Status, Message);
            }
        }
    }
}
