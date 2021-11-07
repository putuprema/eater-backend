namespace Infrastructure.Config
{
    public class DurableFunctionConfig
    {
        public int FirstRetryIntervalSecond { get; set; }
        public int MaxNumberOfAttempts { get; set; }
    }
}
