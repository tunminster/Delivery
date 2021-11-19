using Delivery.Customer.Domain.Contracts.V1.Enums;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification
{
    /// <summary>
    ///  Customer order arrived notification creation contract
    /// </summary>
    public record CustomerOrderNotificationRequestContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        /// Order notification filter
        /// </summary>
        /// <example>1</example>
        public OrderNotificationFilter Filter { get; init; }
    }
}