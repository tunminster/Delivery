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

        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
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

        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
    }
}