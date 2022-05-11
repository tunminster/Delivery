namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderRefund
{
    /// <summary>
    ///  Shop order refund creation contract
    /// </summary>
    public record ShopOrderRefundCreationContract
    {
        /// <summary>
        ///  The order to be refunded
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  The reason of refund
        /// </summary>
        public string Reason { get; init; } = string.Empty;
    }
}