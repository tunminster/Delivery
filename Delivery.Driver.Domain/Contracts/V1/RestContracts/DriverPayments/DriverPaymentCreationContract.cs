namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverPayments
{
    /// <summary>
    ///  Driver payment request contract
    /// </summary>
    public record DriverPaymentCreationContract
    {
        /// <summary>
        ///  Driver connect account id
        /// </summary>
        /// <example>{{driverConnectAccountId}}</example>
        public string DriverConnectAccountId { get; init; } = string.Empty;

        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
    }
}