using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order notification contract
    /// </summary>
    public record ShopOrderNotificationContract : IDataContract
    {
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
        /// <example>{{deliveryAddress}}</example>
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery fee
        /// <example>{{deliveryFee}}</example>
        /// </summary>
        public int DeliveryFee { get; init; }
        
    }
}