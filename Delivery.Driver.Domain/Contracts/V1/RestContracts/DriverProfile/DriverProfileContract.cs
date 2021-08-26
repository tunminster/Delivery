namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Driver profile contract
    /// </summary>
    public record DriverProfileContract
    {
        /// <summary>
        ///  Driver id
        /// <example>{{driverId}}</example>
        /// </summary>
        public string DriverId { get; init; } = string.Empty;

        /// <summary>
        ///  FullName
        /// <example>{{fullName}}</example>
        /// </summary>
        public string FullName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Email Address
        /// <example>{{emailAddress}}</example>
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Address
        /// <example>{{address}}</example>
        /// </summary>
        public string Address { get; init; } = string.Empty;

        /// <summary>
        ///  Profile image uri
        /// <example>{{imageUri}}</example>
        /// </summary>
        public string ImageUri { get; init; } = string.Empty;

    }
}