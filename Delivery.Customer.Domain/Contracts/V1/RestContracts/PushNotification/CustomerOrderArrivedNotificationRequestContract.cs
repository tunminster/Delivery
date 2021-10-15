namespace Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification
{
    /// <summary>
    ///  Customer order arrived notification creation contract
    /// </summary>
    public record CustomerOrderArrivedNotificationRequestContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
    }
}