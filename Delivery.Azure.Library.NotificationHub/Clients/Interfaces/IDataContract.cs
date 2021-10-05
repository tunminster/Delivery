using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Azure.Library.NotificationHub.Clients.Interfaces
{
    public interface IDataContract
    {
        public PushNotificationType PushNotificationType { get; init; }
        public string StoreName { get; init; }
        
        public string StoreId { get; init; }
        
        public string OrderId { get; init; }
        
        public string StoreAddress { get; init; }
        
        public string DeliveryAddress { get; init; }
        
        public int DeliveryFee { get; init; }
    }
}