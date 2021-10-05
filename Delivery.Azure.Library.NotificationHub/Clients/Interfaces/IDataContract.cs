using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Azure.Library.NotificationHub.Clients.Interfaces
{
    public interface IDataContract
    {
        public PushNotificationType PushNotificationType { get; init; }
        public string StoreName { get; init; }
    }
}