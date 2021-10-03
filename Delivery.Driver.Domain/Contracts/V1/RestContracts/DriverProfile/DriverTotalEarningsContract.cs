namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile
{
    /// <summary>
    ///  Total earnings 
    /// </summary>
    public record DriverTotalEarningsContract
    {
        /// <summary>
        ///  Total orders
        /// </summary>
        public int TotalOrders { get; init; }
        
        /// <summary>
        ///  Total earnings 
        /// </summary>
        public int TotalEarnings { get; init; }
    }
}