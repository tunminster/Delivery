namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments
{
    /// <summary>
    ///  Coupon payment creation contract
    /// </summary>
    public record CouponPaymentCreationContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Shop owner connected account
        /// </summary>
        /// <example>{{shopOwnerConnectedAccount}}</example>
        public string ShopOwnerConnectedAccount { get; init; } = string.Empty;
        
        /// <summary>
        ///  Discount amount
        /// </summary>
        /// <example>{{discountAmount}}</example>
        public int DiscountAmount { get; init; }
        
        /// <summary>
        ///  Coupon code
        /// </summary>
        /// <example>{{couponCode}}</example>
        public string CouponCode { get; init; }
    }
}