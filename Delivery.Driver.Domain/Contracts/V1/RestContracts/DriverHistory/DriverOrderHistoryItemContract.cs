namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory
{
    /// <summary>
    ///  Driver order history item contract
    /// </summary>
    public record DriverOrderHistoryItemContract
    {
        /// <summary>
        ///  Item name
        /// </summary>
        /// <example>{{itemName}}</example>
        public string ItemName { get; init; } = string.Empty;

        /// <summary>
        ///  Item count
        /// </summary>
        /// <example>{{itemCount}}</example>
        public string ItemCount { get; init; } = string.Empty;
    }
}