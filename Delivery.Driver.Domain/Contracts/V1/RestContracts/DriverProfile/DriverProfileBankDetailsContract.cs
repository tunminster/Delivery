namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Driver profile bank details contract
    /// </summary>
    public record DriverProfileBankDetailsContract
    {
        /// <summary>
        ///  Driver id
        /// </summary>
        /// <example>{{driverId}}</example>
        public string DriverId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Bank name
        /// </summary>
        /// <example>{{bankName}}</example>
        public string BankName { get; init; } = string.Empty;

        /// <summary>
        ///  Account number
        /// </summary>
        /// <example>{{accountNumber}}</example>
        public string AccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Routing number
        /// </summary>
        /// <example>{{routingNumber}}</example>
        public string RoutingNumber { get; init; } = string.Empty;
    }
}