namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments
{
    /// <summary>
    ///  Coupon code confirmation creation contract
    /// </summary>
    public record CouponCodeConfirmationQueryContract
    {
        /// <summary>
        ///  Coupon code
        /// </summary>
        /// <example>{{couponCode}}</example>
        public string CouponCode { get; init; } = string.Empty;
    }
}