namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection
{
    /// <summary>
    ///  Request another driver contract
    /// </summary>
    public record RequestAnotherDriverContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
    }
}