namespace Domain.Common
{
    public class EventEnvelope<T>
    {
        public string CorrelationId { get; set; }
        public string EventType { get; set; }
        public bool Success { get; set; } = true;
        public T Body { get; set; }
    }
}
