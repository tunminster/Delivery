namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Shop order index creation contract
    /// </summary>
    public record ShopOrderIndexCreationContract
    {
        /// <summary>
        ///  Order external id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
    }
}