namespace Delivery.Azure.Library.NotificationHub.Contracts.V1
{
    public record GcmMessageContract<T>
    {
        public NotificationTitle Notification {get; init;}
        public T Data {get; init;}
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