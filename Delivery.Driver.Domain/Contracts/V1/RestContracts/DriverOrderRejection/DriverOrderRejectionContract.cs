namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection
{
    /// <summary>
    ///  Driver order rejection contract
    /// </summary>
    public record DriverOrderRejectionContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
    }
}