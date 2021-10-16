namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order indexing request contract
    /// </summary>
    public record ShopOrderIndexRequestContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Is all order indexing
        /// </summary>
        public  bool IsAllOrders { get; init; }
    }
}