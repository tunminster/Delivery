namespace Delivery.Order.Domain.Contracts.RestContracts.PushNotification
{
    /// <summary>
    ///  New order request push notification contract
    /// </summary>
    public record OrderCreatedPushNotificationRequestContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
    }
}