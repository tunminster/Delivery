namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.SplitPayments
{
    /// <summary>
    ///  Split payment creation contract
    /// </summary>
    public record SplitPaymentCreationContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Store owner connected account id
        /// </summary>
        /// <example>{{storeOwnerConnectedAccountId}}</example>
        public string StoreOwnerConnectedAccountId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver connected account id
        /// </summary>
        /// <example>{{driverConnectedAccountId</example>
        public string DriverConnectedAccountId { get; init; } = string.Empty;
        
    }
}