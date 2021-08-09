namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive
{
    /// <summary>
    ///  Driver to be online/offline contract
    /// </summary>
    public record DriverActiveCreationContract
    {
        /// <summary>
        ///  Username
        /// </summary>
        public string Username { get; init; } = string.Empty;
        
        /// <summary>
        ///  Is active
        /// </summary>
        public bool IsActive { get; init; }
    }
}