namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverNotifications
{
    /// <summary>
    ///  Driver order notification contract
    /// </summary>
    public record DriverOrderNotificationContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
    }
}