namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Driver who will deliver the order
    /// </summary>
    public record ShopOrderDriverContract
    {
        /// <summary>
        ///  Driver Id
        /// <example>{{driverId}}</example>
        /// </summary>
        public string DriverId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver image uri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver name
        /// <example>{{name}}</example>
        /// </summary>
        public string Name { get; init; } = string.Empty;
    }
}