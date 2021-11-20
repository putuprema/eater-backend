namespace API.Functions.EventHandlers
{
    public class OrderEventHandlerFunctions
    {
        [FunctionName("OrderQueuedEventHandler")]
        public static async Task HandleOrderQueuedEvent(
            [ServiceBusTrigger("order.queued.evt.to.notifsvc", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [SignalR(HubName = nameof(NotificationHub))] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var eventData = EventGridEvent.Parse(new BinaryData(myQueueItem));
            var order = JsonConvert.DeserializeObject<dynamic>(eventData.Data.ToString());

            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = Events.OrderQueued,
                GroupName = NotificationGroups.KitchenNotificationGroup,
                Arguments = new[] { order }
            });
        }

        [FunctionName("OrderStatusChangedEventHandler")]
        public static async Task HandleOrderStatusChangedEvent(
            [ServiceBusTrigger("order.status.changed.to.notifsvc", Connection = AppSettingsKeys.ServiceBusConnString)] string myQueueItem,
            [SignalR(HubName = nameof(NotificationHub))] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var eventData = EventGridEvent.Parse(new BinaryData(myQueueItem));
            var order = JsonConvert.DeserializeObject<dynamic>(eventData.Data.ToString());

            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = Events.OrderStatusChanged,
                GroupName = eventData.Subject,
                Arguments = new[] { order }
            });
        }
    }
}
