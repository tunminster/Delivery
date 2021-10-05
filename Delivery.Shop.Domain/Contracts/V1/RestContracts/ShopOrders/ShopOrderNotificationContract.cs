using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order notification contract
    /// </summary>
    public record ShopOrderNotificationContract : IDataContract
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
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
    }
}