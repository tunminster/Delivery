namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver order index creation contract
    /// </summary>
    public record DriverOrderIndexCreationContract
    {
        /// <summary>
        /// Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        /// Driver id
        /// </summary>
        /// <example>{{driverId}}</example>
        public string DriverId { get; init; } = string.Empty;
    }
}