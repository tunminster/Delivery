namespace Delivery.Order.Domain.Contracts.V1.RestContracts.PushNotification
{
    /// <summary>
    ///  order item contract
    /// </summary>
    public record OrderCreatedItemContract
    {
        /// <summary>
        ///  Item name
        /// <example>{{itemName}}</example>
        /// </summary>
        public string ItemName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Price
        /// <example>{{price}}</example>
        /// </summary>
        public double Price { get; init; }
        
        /// <summary>
        ///  Count
        /// <example>{{count}}</example>
        /// </summary>
        public int Count { get; init; }
    }
}