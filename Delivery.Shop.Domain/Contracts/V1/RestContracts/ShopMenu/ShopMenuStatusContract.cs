namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu
{
    /// <summary>
    ///  Shop menu status contract
    /// </summary>
    public record ShopMenuStatusContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        public string ProductId { get; init; } = string.Empty;

        /// <summary>
        ///  Product name
        /// </summary>
        public string ProductName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Product image
        /// </summary>
        public string ProductImage { get; init; } = string.Empty;

        /// <summary>
        ///  Price
        /// </summary>
        public int UnitPrice { get; init; }
        
        /// <summary>
        ///  Status
        /// </summary>
        public bool Status { get; init; }
    }
}