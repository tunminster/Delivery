namespace Delivery.Order.Domain.Contracts.V1.MessageContracts.CouponPayment
{
    /// <summary>
    ///  Coupon payment creation message contract
    /// </summary>
    public record CouponPaymentCreationMessageContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Discount amount that used by discount code
        /// </summary>
        public int DiscountAmount { get; init; }

        /// <summary>
        ///  Shop owner connect account
        /// </summary>
        public string ShopOwnerConnectAccount { get; init; } = string.Empty;

        /// <summary>
        ///  Coupon code
        /// </summary>
        public string CouponCode { get; init; } = string.Empty;

    }
}