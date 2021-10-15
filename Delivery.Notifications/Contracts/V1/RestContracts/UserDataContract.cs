using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    /// <summary>
    ///  User data contract
    /// </summary>
    public record UserDataContract : NotificationDataContract
    {
        /// <summary>
        ///  Message
        /// </summary>
        public string Message { get; init; } = string.Empty;
        
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