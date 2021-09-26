namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu
{
    /// <summary>
    ///  Shop menu status creation contract
    /// </summary>
    public record ShopMenuStatusCreationContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        public string? ProductId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Status
        /// </summary>
        public bool Status { get; init; }
    }
}