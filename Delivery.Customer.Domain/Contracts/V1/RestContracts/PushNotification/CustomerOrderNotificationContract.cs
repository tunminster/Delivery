using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification
{
    public record CustomerOrderNotificationContract : IDataContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

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

        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
    }
}