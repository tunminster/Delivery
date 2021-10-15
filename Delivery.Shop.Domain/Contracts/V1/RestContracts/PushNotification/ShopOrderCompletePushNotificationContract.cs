using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.PushNotification
{
    public record ShopOrderCompletePushNotificationContract : NotificationDataContract
    {
        
        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        ///  store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;

        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
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
        ///  Delivery fee
        /// </summary>
        public int DeliveryFee { get; init; }
    }
}