namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Driver profile update contract
    /// </summary>
    public record DriverProfileUpdateContract
    {
        /// <summary>
        ///  Driver id
        /// </summary>
        /// <example>{{driverId}}</example>
        public string DriverId { get; init; } = string.Empty;
    }
}