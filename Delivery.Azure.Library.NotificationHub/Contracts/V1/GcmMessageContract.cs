namespace Delivery.Azure.Library.NotificationHub.Contracts.V1
{
    public record GcmMessageContract
    {
        public NotificationTitle Notification {get; init;}
        public NotificationData Data {get; init;}
    }

    public record NotificationTitle
    {
        public string Title {get; init;}
        public string Body {get; init;}
    }

    public record NotificationData
    {
        public string PropertyOne {get; init;}
        public string PropertyTwo {get; init;}
    }
}