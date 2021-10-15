using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Azure.Library.NotificationHub.Clients.Contracts
{
    /// <summary>
    ///  Notification data contract
    /// </summary>
    public record NotificationDataContract
    {
        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
        
        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; }
        
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; }
        
        /// <summary>
        ///  Orderid
        /// </summary>
        public string OrderId { get; init; }
    }
}