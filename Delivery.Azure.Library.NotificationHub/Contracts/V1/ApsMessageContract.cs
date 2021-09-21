namespace Delivery.Azure.Library.NotificationHub.Contracts.V1
{
    public record ApsMessageContract<T>
    {
        public Aps Aps { get; init; }
        
        public ApsNotificationMessage Message { get; init; }
        public T Data { get; init; }
    }

    public record Aps
    {
        public string Alert {get; init;}
        
        public string Sound { get; init; } = "default";
    }

    public record ApsNotificationMessage
    {
        public string Message { get; init; }
    }
}