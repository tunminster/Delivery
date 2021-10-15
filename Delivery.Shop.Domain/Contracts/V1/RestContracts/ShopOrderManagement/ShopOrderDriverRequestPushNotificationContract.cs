using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Contract send to push message to driver
    /// </summary>
    public record ShopOrderDriverRequestPushNotificationContract : NotificationDataContract
    {
        /// <summary>
        ///  Store image uri
        /// </summary>
        /// <example>{{storeImageUri}}</example>
        public string StoreImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store address
        /// </summary>
        /// <example>{{storeAddress}}</example>
        public string StoreAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery address
        /// <example>{{deliveryAddress}}</example>
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery fee
        /// <example>{{deliveryFee}}</example>
        /// </summary>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Tips
        /// <example>{{deliveryTips}}</example>
        /// </summary>
        public int DeliveryTips { get; init; }
    }
}