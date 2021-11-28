namespace Eater.Shared.Common
{
    /// <summary>
    /// Class to encapsulate events
    /// </summary>
    /// <param name="CorrelationId">A unique ID to identify the transaction associated to this event</param>
    /// <param name="Subject">Subject of the event, can be the original command that produces the event or what this event is about (e.g. InitPayment, PaymentStatus)</param>
    /// <param name="EventType">Type of the event (e.g. InitPaymentSuccess, InitPaymentFailed, PaymentStatusChanged)</param>
    /// <param name="Body">Body of the event</param>
    public class EventEnvelope<T>
    {
        /// <summary>
        /// A unique ID to identify the transaction associated to this event
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Subject of the event, can be the original command that produces the event
        /// Or what this event is about (e.g. InitPayment, PaymentStatus)
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Type of the event (e.g. InitPaymentSuccess, InitPaymentFailed, PaymentStatusChanged)
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Body of the event
        /// </summary>
        public T? Body { get; set; }

        /// <param name="correlationId">A unique ID to identify the transaction associated to this event</param>
        /// <param name="subject">Subject of the event, can be the original command that produces the event or what this event is about (e.g. InitPayment, PaymentStatus)</param>
        /// <param name="eventType">Type of the event (e.g. InitPaymentSuccess, InitPaymentFailed, PaymentStatusChanged)</param>
        /// <param name="body">Body of the event</param>
        public EventEnvelope(string correlationId, string subject, string eventType, T? body)
        {
            CorrelationId = correlationId;
            Subject = subject;
            EventType = eventType;
            Body = body;
        }
    }

    public static class Program
    {
        public static void Main()
        {
            var test = new EventEnvelope<dynamic>("", "", "", "");
        }
    }
}
