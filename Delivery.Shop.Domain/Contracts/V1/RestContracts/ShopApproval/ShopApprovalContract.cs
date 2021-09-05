namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopApproval
{
    /// <summary>
    ///  Shop approval contract
    /// </summary>
    public record ShopApprovalContract
    {
        /// <summary>
        ///  Shop id
        /// <example>{{shopId}}</example>
        /// </summary>
        public string ShopId { get; init; } = string.Empty;
    }
}