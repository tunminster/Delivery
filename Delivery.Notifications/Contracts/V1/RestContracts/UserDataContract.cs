using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    /// <summary>
    ///  User data contract
    /// </summary>
    public record UserDataContract : IDataContract
    {
        /// <summary>
        ///  Message
        /// </summary>
        public string Message { get; init; } = string.Empty;

        public PushNotificationType PushNotificationType { get; init; }
        public string StoreName { get; init; }
        public string StoreId { get; init; }
        public string OrderId { get; init; }

        /// <summary>
        ///  Store address
        /// </summary>
        public string StoreAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery address
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery fees
        /// </summary>
        public int DeliveryFee { get; init; }
        
    }
}